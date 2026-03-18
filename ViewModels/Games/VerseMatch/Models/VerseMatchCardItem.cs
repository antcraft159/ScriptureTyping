using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ScriptureTyping.ViewModels.Games.VerseMatch.Models
{
    /// <summary>
    /// 목적:
    /// VerseMatch 게임 카드 1장을 나타낸다.
    ///
    /// 호환성:
    /// - 기존 코드에서 사용하던 CardId, Reset, Select, Unselect, MarkMatched 지원
    /// - 생성자 named argument로 displayText / fullText 둘 다 지원
    /// - VerseText / FullText / DisplayText 동시 지원
    /// </summary>
    public sealed class VerseMatchCardItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isMatched;
        private string _displayText = string.Empty;

        /// <summary>
        /// 목적:
        /// 카드 식별자이다.
        /// 기존 코드 호환용이다.
        /// </summary>
        public int CardId { get; set; }

        /// <summary>
        /// 목적:
        /// 같은 PairKey를 가진 Reference 카드와 VerseText 카드가 한 쌍이다.
        /// </summary>
        public string PairKey { get; }

        /// <summary>
        /// 목적:
        /// 카드 종류를 나타낸다.
        /// </summary>
        public VerseMatchCardType CardType { get; }

        /// <summary>
        /// 목적:
        /// 화면에 표시할 텍스트이다.
        /// </summary>
        public string DisplayText
        {
            get => _displayText;
            private set
            {
                if (_displayText == value)
                {
                    return;
                }

                _displayText = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(VerseText));
                OnPropertyChanged(nameof(FullText));
            }
        }

        /// <summary>
        /// 목적:
        /// 기존 코드 호환용 본문 텍스트 별칭이다.
        /// </summary>
        public string VerseText => DisplayText;

        /// <summary>
        /// 목적:
        /// 기존 코드 호환용 전체 텍스트 별칭이다.
        /// </summary>
        public string FullText => DisplayText;

        /// <summary>
        /// 목적:
        /// 현재 카드가 장절 카드인지 여부를 나타낸다.
        /// </summary>
        public bool IsReferenceCard => CardType == VerseMatchCardType.Reference;

        /// <summary>
        /// 목적:
        /// 현재 카드가 본문 카드인지 여부를 나타낸다.
        /// </summary>
        public bool IsContentCard =>
            CardType == VerseMatchCardType.Content ||
            CardType == VerseMatchCardType.VerseText;

        /// <summary>
        /// 목적:
        /// 카드 상단 라벨 텍스트를 제공한다.
        /// </summary>
        public string CardLabelText => IsReferenceCard ? "장절" : "본문";

        /// <summary>
        /// 목적:
        /// 현재 카드가 선택되었는지 여부를 나타낸다.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            private set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 목적:
        /// 현재 카드가 매칭 완료되었는지 여부를 나타낸다.
        /// </summary>
        public bool IsMatched
        {
            get => _isMatched;
            private set
            {
                if (_isMatched == value)
                {
                    return;
                }

                _isMatched = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 목적:
        /// VerseMatchCardItem을 생성한다.
        ///
        /// 주의:
        /// - displayText named argument 지원
        /// - fullText named argument 지원
        /// - 둘 다 들어오면 displayText를 우선 사용
        /// </summary>
        public VerseMatchCardItem(
            string pairKey,
            VerseMatchCardType cardType,
            string displayText = "",
            string fullText = "",
            int cardId = 0)
        {
            PairKey = pairKey ?? string.Empty;
            CardType = cardType;
            CardId = cardId;
            DisplayText = string.IsNullOrWhiteSpace(displayText) ? (fullText ?? string.Empty) : displayText;
        }

        /// <summary>
        /// 목적:
        /// 카드를 선택 상태로 만든다.
        /// </summary>
        public void Select()
        {
            if (IsMatched)
            {
                return;
            }

            IsSelected = true;
        }

        /// <summary>
        /// 목적:
        /// 카드 선택을 해제한다.
        /// </summary>
        public void Unselect()
        {
            if (IsMatched)
            {
                return;
            }

            IsSelected = false;
        }

        /// <summary>
        /// 목적:
        /// 카드를 매칭 완료 상태로 만든다.
        /// </summary>
        public void MarkMatched()
        {
            IsMatched = true;
            IsSelected = false;
        }

        /// <summary>
        /// 목적:
        /// 카드 상태를 초기화한다.
        /// </summary>
        public void Reset()
        {
            IsSelected = false;
            IsMatched = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}