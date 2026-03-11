using ScriptureTyping.Commands;
using ScriptureTyping.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace ScriptureTyping.ViewModels.Games
{
    public sealed partial class ClozeGameViewModel
    {
        private RelayCommand? _submitVeryHardAnswerCommand;

        public sealed class VeryHardInputItem
        {
            private string _value = string.Empty;

            public string Label { get; init; } = string.Empty;
            public string Answer { get; init; } = string.Empty;

            public string Value
            {
                get => _value;
                set => _value = value ?? string.Empty;
            }
        }

        public ObservableCollection<VeryHardInputItem> VeryHardInputs { get; } = new ObservableCollection<VeryHardInputItem>();

        public bool IsVeryHardInputVisible =>
            IsVeryHardDifficulty() &&
            _current != null &&
            VeryHardInputs.Count > 0;

        public bool CanSubmitVeryHardAnswer =>
            IsVeryHardInputVisible &&
            CanAnswerChoices();

        public ICommand SubmitVeryHardAnswerCommand =>
            _submitVeryHardAnswerCommand ??= new RelayCommand(_ => SubmitVeryHardAnswer(), _ => CanSubmitVeryHardAnswer);

        private bool IsVeryHardDifficulty()
        {
            return CurrentDifficulty == DIFFICULTY_VERY_HARD;
        }

        private int GetVeryHardBlankCount()
        {
            return 5;
        }

        private int GetVeryHardChoiceCount()
        {
            return 0;
        }

        private int GetVeryHardTryCount()
        {
            return 1;
        }

        private int GetVeryHardCorrectScore()
        {
            return 16;
        }

        private int GetVeryHardWrongPenalty()
        {
            return 4;
        }

        private bool IsVeryHardTimeAttack()
        {
            return true;
        }

        private int GetVeryHardTimeAttackSeconds()
        {
            return 10;
        }

        private bool TryMakeVeryHardQuestion(VerseItem verse, out ClozeQuestion? question)
        {
            question = null;

            if (string.IsNullOrWhiteSpace(verse.Text) || string.IsNullOrWhiteSpace(verse.Ref))
            {
                return false;
            }

            List<string> candidates = ExtractCandidateWords(verse.Text)
                .Distinct(StringComparer.Ordinal)
                .OrderByDescending(x => x.Length)
                .ThenBy(_ => _rng.Next())
                .ToList();

            if (candidates.Count < 2)
            {
                return false;
            }

            if (!TrySplitReference(verse.Ref, out string book, out string chapter, out string verseNumber))
            {
                return false;
            }

            List<string> selectedTextAnswers = candidates
                .Take(2)
                .ToList();

            List<string> orderedTextAnswers = OrderAnswersByAppearance(verse.Text, selectedTextAnswers);
            if (orderedTextAnswers.Count != 2)
            {
                return false;
            }

            if (!TryBuildClozeText(verse.Text, orderedTextAnswers, out string clozeText))
            {
                return false;
            }

            string maskedReference = $"{BuildMaskedReferencePart(book)} {BuildMaskedReferencePart(chapter)}:{BuildMaskedReferencePart(verseNumber)}";

            List<string> allAnswers = new List<string>();
            allAnswers.AddRange(orderedTextAnswers);
            allAnswers.Add(book);
            allAnswers.Add(chapter);
            allAnswers.Add(verseNumber);

            question = new ClozeQuestion
            {
                Reference = maskedReference,
                OriginalReference = verse.Ref,
                OriginalText = verse.Text,
                ClozeText = clozeText,
                Answers = allAnswers,
                ChoiceSets = Array.Empty<IReadOnlyList<string>>()
            };

            return true;
        }

        private void InitializeVeryHardInputs(ClozeQuestion question)
        {
            ClearVeryHardInputs();

            if (question.Answers.Count < 5)
            {
                return;
            }

            VeryHardInputs.Add(new VeryHardInputItem { Label = "빈칸 1", Answer = question.Answers[0] });
            VeryHardInputs.Add(new VeryHardInputItem { Label = "빈칸 2", Answer = question.Answers[1] });
            VeryHardInputs.Add(new VeryHardInputItem { Label = "권", Answer = question.Answers[2] });
            VeryHardInputs.Add(new VeryHardInputItem { Label = "장", Answer = question.Answers[3] });
            VeryHardInputs.Add(new VeryHardInputItem { Label = "절", Answer = question.Answers[4] });

            OnPropertyChanged(nameof(VeryHardInputs));
            OnPropertyChanged(nameof(IsVeryHardInputVisible));
            OnPropertyChanged(nameof(CanSubmitVeryHardAnswer));
        }

        private void ClearVeryHardInputs()
        {
            VeryHardInputs.Clear();
            OnPropertyChanged(nameof(VeryHardInputs));
            OnPropertyChanged(nameof(IsVeryHardInputVisible));
            OnPropertyChanged(nameof(CanSubmitVeryHardAnswer));
        }

        private void SubmitVeryHardAnswer()
        {
            if (!CanSubmitVeryHardAnswer || _current == null)
            {
                return;
            }

            bool allCorrect = true;

            for (int i = 0; i < VeryHardInputs.Count; i++)
            {
                string expected = NormalizeVeryHardAnswer(VeryHardInputs[i].Answer);
                string actual = NormalizeVeryHardAnswer(VeryHardInputs[i].Value);

                if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
                {
                    allCorrect = false;
                    break;
                }
            }

            if (allCorrect)
            {
                HandleCorrectAnswer();
                return;
            }

            _isCorrect = false;
            _tryLeft -= 1;
            _score = Math.Max(0, _score - GetWrongPenalty());
            _combo = 0;

            if (_tryLeft <= 0)
            {
                FeedbackText = $"기회 소진. 정답은 {BuildVeryHardAnswerSummary(_current)} 입니다.";
                StopTimer();
                RaiseUiComputed();
                ScheduleAutoNext();
                return;
            }

            FeedbackText = $"틀렸습니다. 다시 입력하세요. (기회 {_tryLeft}회)";
            RaiseUiComputed();
        }

        private void ApplyVeryHardHint()
        {
            if (_current == null)
            {
                return;
            }

            for (int i = 0; i < VeryHardInputs.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(VeryHardInputs[i].Value))
                {
                    continue;
                }

                string answer = VeryHardInputs[i].Answer;
                if (string.IsNullOrWhiteSpace(answer))
                {
                    continue;
                }

                VeryHardInputs[i].Value = answer.Substring(0, 1);
            }

            QuestionText = ReplacePlaceholderByOrder(_current.ClozeText, 0, $"{FirstHintChar(_current.Answers.ElementAtOrDefault(0))}…");
            QuestionText = ReplacePlaceholderByOrder(QuestionText, 1, $"{FirstHintChar(_current.Answers.ElementAtOrDefault(1))}…");
            ReferenceText = $"{FirstHintChar(_current.Answers.ElementAtOrDefault(2))}… {FirstHintChar(_current.Answers.ElementAtOrDefault(3))}…:{FirstHintChar(_current.Answers.ElementAtOrDefault(4))}…";

            OnPropertyChanged(nameof(VeryHardInputs));
        }

        private string BuildVeryHardAnswerSummary(ClozeQuestion question)
        {
            if (question.Answers.Count < 5)
            {
                return string.Join(", ", question.Answers);
            }

            return $"본문: {question.Answers[0]}, {question.Answers[1]} / 장절: {question.Answers[2]} {question.Answers[3]}:{question.Answers[4]}";
        }

        private static string NormalizeVeryHardAnswer(string value)
        {
            return (value ?? string.Empty).Trim();
        }

        private static string FirstHintChar(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.Substring(0, 1);
        }

        private static bool TrySplitReference(string reference, out string book, out string chapter, out string verseNumber)
        {
            book = string.Empty;
            chapter = string.Empty;
            verseNumber = string.Empty;

            if (string.IsNullOrWhiteSpace(reference))
            {
                return false;
            }

            string trimmed = reference.Trim();
            int lastSpaceIndex = trimmed.LastIndexOf(' ');
            if (lastSpaceIndex <= 0 || lastSpaceIndex >= trimmed.Length - 1)
            {
                return false;
            }

            book = trimmed.Substring(0, lastSpaceIndex).Trim();
            string chapterVerse = trimmed.Substring(lastSpaceIndex + 1).Trim();

            string[] parts = chapterVerse.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            chapter = parts[0].Trim();
            verseNumber = parts[1].Trim();

            return !string.IsNullOrWhiteSpace(book) &&
                   !string.IsNullOrWhiteSpace(chapter) &&
                   !string.IsNullOrWhiteSpace(verseNumber);
        }

        private static string BuildMaskedReferencePart(string value)
        {
            int length = Math.Max(2, (value ?? string.Empty).Trim().Length);
            return new string('_', length);
        }
    }
}