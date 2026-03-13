using ScriptureTyping.Commands;
using ScriptureTyping.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace ScriptureTyping.ViewModels.Games
{
    public sealed partial class ClozeGameViewModel
    {
        private RelayCommand? _submitVeryHardAnswerCommand;

        private const int VERY_HARD_REFERENCE_PART_COUNT = 3;

        public sealed class VeryHardInputItem
        {
            private string _value = string.Empty;

            public string Label { get; init; } = string.Empty;

            public string Answer { get; init; } = string.Empty;

            public int StartBlankIndex { get; init; }

            public int EndBlankIndex { get; init; }

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
            _submitVeryHardAnswerCommand ??= new RelayCommand(
                _ => SubmitVeryHardAnswer(),
                _ => CanSubmitVeryHardAnswer);

        private bool IsVeryHardDifficulty()
        {
            return CurrentDifficulty == DIFFICULTY_VERY_HARD;
        }

        private int GetVeryHardBlankCount()
        {
            return 0;
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
            return 90;
        }

        private bool TryMakeVeryHardQuestion(VerseItem verse, out ClozeQuestion? question)
        {
            question = null;

            if (string.IsNullOrWhiteSpace(verse.Text) || string.IsNullOrWhiteSpace(verse.Ref))
            {
                return false;
            }

            List<string> tokenizedWords = TokenizeVeryHardWords(verse.Text);
            List<int> candidateIndexes = GetVeryHardCandidateIndexes(tokenizedWords);

            if (candidateIndexes.Count == 0)
            {
                return false;
            }

            if (!TrySplitReference(verse.Ref, out string book, out string chapter, out string verseNumber))
            {
                return false;
            }

            int textBlankCount = ResolveVeryHardTextBlankCount(tokenizedWords.Count, candidateIndexes.Count);

            if (textBlankCount <= 0)
            {
                return false;
            }

            List<int> selectedIndexes = candidateIndexes
                .OrderByDescending(index => tokenizedWords[index].Length)
                .ThenBy(_ => _rng.Next())
                .Take(textBlankCount)
                .OrderBy(index => index)
                .ToList();

            if (selectedIndexes.Count == 0)
            {
                return false;
            }

            List<string> orderedTextAnswers = selectedIndexes
                .Select(index => tokenizedWords[index])
                .ToList();

            if (!TryBuildVeryHardGroupedClozeText(verse.Text, orderedTextAnswers, out string clozeText))
            {
                return false;
            }

            string maskedReference =
                $"{BuildMaskedReferencePart(book)} {BuildMaskedReferencePart(chapter)}:{BuildMaskedReferencePart(verseNumber)}";

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

        private bool TryBuildVeryHardGroupedClozeText(
            string text,
            IReadOnlyList<string> answers,
            out string clozeText)
        {
            clozeText = text;

            if (string.IsNullOrWhiteSpace(text) || answers == null || answers.Count == 0)
            {
                return false;
            }

            List<ReplacementTarget> targets = new List<ReplacementTarget>();
            int searchStartIndex = 0;

            for (int i = 0; i < answers.Count; i++)
            {
                string answer = answers[i];
                int index = text.IndexOf(answer, searchStartIndex, StringComparison.Ordinal);

                if (index < 0)
                {
                    clozeText = text;
                    return false;
                }

                targets.Add(new ReplacementTarget(index, answer, i + 1));
                searchStartIndex = index + answer.Length;
            }

            List<List<ReplacementTarget>> groups = BuildVeryHardReplacementGroups(text, targets);
            string result = text;

            List<(List<ReplacementTarget> Group, int DisplayOrder)> indexedGroups = groups
                .Select((group, index) => (Group: group, DisplayOrder: index + 1))
                .OrderByDescending(x => x.Group.First().Index)
                .ToList();

            foreach ((List<ReplacementTarget> Group, int DisplayOrder) indexedGroup in indexedGroups)
            {
                List<ReplacementTarget> group = indexedGroup.Group;
                int displayOrder = indexedGroup.DisplayOrder;

                int startIndex = group.First().Index;
                int endIndex = group.Last().Index + group.Last().Answer.Length;

                if (endIndex < startIndex)
                {
                    clozeText = text;
                    return false;
                }

                string originalSegment = text.Substring(startIndex, endIndex - startIndex);
                string mergedBlank = BuildVeryHardMergedBlank(originalSegment.Length);
                string replacement = $"[{displayOrder}] {mergedBlank}";

                result = result.Substring(0, startIndex)
                       + replacement
                       + result.Substring(endIndex);
            }

            if (string.Equals(result, text, StringComparison.Ordinal))
            {
                clozeText = text;
                return false;
            }

            clozeText = result;
            return true;
        }

        private List<List<ReplacementTarget>> BuildVeryHardReplacementGroups(
            string originalText,
            IReadOnlyList<ReplacementTarget> orderedTargets)
        {
            List<List<ReplacementTarget>> result = new List<List<ReplacementTarget>>();

            if (orderedTargets.Count == 0)
            {
                return result;
            }

            List<ReplacementTarget> currentGroup = new List<ReplacementTarget>
            {
                orderedTargets[0]
            };

            for (int i = 1; i < orderedTargets.Count; i++)
            {
                ReplacementTarget previous = orderedTargets[i - 1];
                ReplacementTarget current = orderedTargets[i];

                int previousEnd = previous.Index + previous.Answer.Length;
                int betweenLength = current.Index - previousEnd;

                if (betweenLength < 0)
                {
                    result.Add(currentGroup);
                    currentGroup = new List<ReplacementTarget> { current };
                    continue;
                }

                string betweenText = betweenLength > 0
                    ? originalText.Substring(previousEnd, betweenLength)
                    : string.Empty;

                bool hasOnlyWhitespaceBetween = string.IsNullOrWhiteSpace(betweenText);

                if (hasOnlyWhitespaceBetween)
                {
                    currentGroup.Add(current);
                    continue;
                }

                result.Add(currentGroup);
                currentGroup = new List<ReplacementTarget> { current };
            }

            result.Add(currentGroup);

            return result;
        }

        private string BuildVeryHardMergedBlank(int originalLength)
        {
            int underscoreLength = Math.Max(10, originalLength + 4);
            return new string('_', underscoreLength);
        }

        private void InitializeVeryHardInputs(ClozeQuestion question)
        {
            ClearVeryHardInputs();

            if (question.Answers.Count < VERY_HARD_REFERENCE_PART_COUNT)
            {
                return;
            }

            int textAnswerCount = question.Answers.Count - VERY_HARD_REFERENCE_PART_COUNT;

            if (textAnswerCount <= 0)
            {
                return;
            }

            List<VeryHardInputItem> groupedInputs = BuildVeryHardGroupedInputs(question, textAnswerCount);

            foreach (VeryHardInputItem item in groupedInputs)
            {
                VeryHardInputs.Add(item);
            }

            VeryHardInputs.Add(new VeryHardInputItem
            {
                Label = "권",
                Answer = question.Answers[textAnswerCount]
            });

            VeryHardInputs.Add(new VeryHardInputItem
            {
                Label = "장",
                Answer = question.Answers[textAnswerCount + 1]
            });

            VeryHardInputs.Add(new VeryHardInputItem
            {
                Label = "절",
                Answer = question.Answers[textAnswerCount + 2]
            });

            OnPropertyChanged(nameof(VeryHardInputs));
            OnPropertyChanged(nameof(IsVeryHardInputVisible));
            OnPropertyChanged(nameof(CanSubmitVeryHardAnswer));
        }

        private List<VeryHardInputItem> BuildVeryHardGroupedInputs(ClozeQuestion question, int textAnswerCount)
        {
            List<VeryHardInputItem> result = new List<VeryHardInputItem>();

            List<int> groupStartOrders = ExtractVeryHardGroupStartOrders(question.ClozeText);
            if (groupStartOrders.Count == 0)
            {
                return result;
            }

            List<(int Start, int End)> ranges = BuildVeryHardRangesFromStarts(groupStartOrders, textAnswerCount);

            for (int i = 0; i < ranges.Count; i++)
            {
                int start = ranges[i].Start;
                int end = ranges[i].End;

                if (start < 1 || end > textAnswerCount || start > end)
                {
                    continue;
                }

                int skip = start - 1;
                int take = end - start + 1;

                string groupedAnswer = string.Join(" ", question.Answers.Skip(skip).Take(take));

                result.Add(new VeryHardInputItem
                {
                    Label = $"입력 {i + 1}",
                    Answer = groupedAnswer,
                    StartBlankIndex = start,
                    EndBlankIndex = end
                });
            }

            return result;
        }

        private List<int> ExtractVeryHardGroupStartOrders(string clozeText)
        {
            List<int> result = new List<int>();

            if (string.IsNullOrWhiteSpace(clozeText))
            {
                return result;
            }

            MatchCollection matches = Regex.Matches(clozeText, @"\[(\d+)\]\s*_+");

            foreach (Match match in matches)
            {
                if (!match.Success)
                {
                    continue;
                }

                if (int.TryParse(match.Groups[1].Value, out int order))
                {
                    result.Add(order);
                }
            }

            return result;
        }

        private List<(int Start, int End)> BuildVeryHardRangesFromStarts(
            IReadOnlyList<int> groupStartOrders,
            int textAnswerCount)
        {
            List<(int Start, int End)> result = new List<(int Start, int End)>();

            if (groupStartOrders.Count == 0 || textAnswerCount <= 0)
            {
                return result;
            }

            for (int i = 0; i < groupStartOrders.Count; i++)
            {
                int start = groupStartOrders[i];
                int end = i < groupStartOrders.Count - 1
                    ? groupStartOrders[i + 1] - 1
                    : textAnswerCount;

                result.Add((start, end));
            }

            return result;
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

            FeedbackText = $"문장의 빈칸을 직접 입력하세요. (기회 {_tryLeft}회)";
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

            int textAnswerCount = Math.Max(0, _current.Answers.Count - VERY_HARD_REFERENCE_PART_COUNT);

            List<VeryHardInputItem> groupedInputs = BuildVeryHardGroupedInputs(_current, textAnswerCount);
            string hintedQuestionText = _current.ClozeText;

            for (int i = 0; i < groupedInputs.Count; i++)
            {
                hintedQuestionText = ReplacePlaceholderByOrder(
                    hintedQuestionText,
                    i,
                    $"{FirstHintChar(groupedInputs[i].Answer)}…");
            }

            QuestionText = hintedQuestionText;

            if (_current.Answers.Count >= VERY_HARD_REFERENCE_PART_COUNT)
            {
                int bookIndex = textAnswerCount;
                int chapterIndex = textAnswerCount + 1;
                int verseIndex = textAnswerCount + 2;

                ReferenceText =
                    $"{FirstHintChar(_current.Answers.ElementAtOrDefault(bookIndex))}… " +
                    $"{FirstHintChar(_current.Answers.ElementAtOrDefault(chapterIndex))}…:" +
                    $"{FirstHintChar(_current.Answers.ElementAtOrDefault(verseIndex))}…";
            }

            OnPropertyChanged(nameof(VeryHardInputs));
        }

        private string BuildVeryHardAnswerSummary(ClozeQuestion question)
        {
            if (question.Answers.Count < VERY_HARD_REFERENCE_PART_COUNT)
            {
                return string.Join(", ", question.Answers);
            }

            int textAnswerCount = question.Answers.Count - VERY_HARD_REFERENCE_PART_COUNT;

            if (textAnswerCount <= 0)
            {
                return string.Join(", ", question.Answers);
            }

            List<string> groupedTextAnswers = BuildVeryHardGroupedInputs(question, textAnswerCount)
                .Select(item => item.Answer)
                .ToList();

            string textAnswers = groupedTextAnswers.Count > 0
                ? string.Join(" / ", groupedTextAnswers)
                : string.Join(", ", question.Answers.Take(textAnswerCount));

            string book = question.Answers[textAnswerCount];
            string chapter = question.Answers[textAnswerCount + 1];
            string verse = question.Answers[textAnswerCount + 2];

            return $"본문: {textAnswers} / 장절: {book} {chapter}:{verse}";
        }

        private static string NormalizeVeryHardAnswer(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return Regex.Replace(value.Trim(), @"\s+", " ");
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

        private List<string> TokenizeVeryHardWords(string text)
        {
            return text
                .Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(word => TrimPunctuation(word.Trim()))
                .Where(word => !string.IsNullOrWhiteSpace(word))
                .ToList();
        }

        private List<int> GetVeryHardCandidateIndexes(IReadOnlyList<string> tokens)
        {
            List<int> result = new List<int>();

            if (tokens.Count == 0)
            {
                return result;
            }

            int start = tokens.Count >= 3 ? 1 : 0;
            int end = tokens.Count >= 3 ? tokens.Count - 2 : tokens.Count - 1;

            for (int i = start; i <= end; i++)
            {
                if (i < 0 || i >= tokens.Count)
                {
                    continue;
                }

                string token = tokens[i];

                if (token.Length < 2)
                {
                    continue;
                }

                if (token.All(char.IsDigit))
                {
                    continue;
                }

                result.Add(i);
            }

            if (result.Count > 0)
            {
                return result;
            }

            for (int i = 0; i < tokens.Count; i++)
            {
                string token = tokens[i];

                if (token.Length < 2)
                {
                    continue;
                }

                if (token.All(char.IsDigit))
                {
                    continue;
                }

                result.Add(i);
            }

            return result;
        }

        private int ResolveVeryHardTextBlankCount(int totalTokenCount, int candidateCount)
        {
            if (candidateCount <= 0)
            {
                return 0;
            }

            if (GetVeryHardBlankCount() > 0)
            {
                return Math.Min(GetVeryHardBlankCount(), candidateCount);
            }

            double ratio = GetVeryHardMaskRatio(totalTokenCount);
            int resolved = (int)Math.Round(candidateCount * ratio, MidpointRounding.AwayFromZero);

            if (resolved < 1)
            {
                resolved = 1;
            }

            if (resolved > candidateCount)
            {
                resolved = candidateCount;
            }

            return resolved;
        }

        private double GetVeryHardMaskRatio(int totalTokenCount)
        {
            if (totalTokenCount <= 4)
            {
                return 0.60;
            }

            if (totalTokenCount <= 6)
            {
                return 0.70;
            }

            if (totalTokenCount <= 10)
            {
                return 0.80;
            }

            return 0.90;
        }
    }
}