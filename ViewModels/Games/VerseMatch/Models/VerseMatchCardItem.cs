using System;

namespace ScriptureTyping.ViewModels.Games.VerseMatch.Models
{
    /// <summary>
    /// 목적:
    /// 구절 짝 맞추기 게임에서 카드 1장의 상태를 표현한다.
    ///
    /// 역할:
    /// - 화면에 표시할 텍스트 보관
    /// - 장절 카드인지 본문 카드인지 구분
    /// - 어떤 카드와 짝인지 식별
    /// - 선택/매치 상태 관리
    /// </summary>
    public sealed class VerseMatchCardItem : BaseViewModel
    {
        private bool _isSelected;
        private bool _isMatched;

        /// <summary>
        /// 목적:
        /// 카드 고유 식별자
        /// </summary>
        public Guid CardId { get; } = Guid.NewGuid();

        /// <summary>
        /// 목적:
        /// 짝을 이루는 카드끼리 공유하는 키
        /// </summary>
        public string PairKey { get; }

        /// <summary>
        /// 목적:
        /// 화면에 표시할 카드 텍스트
        /// </summary>
        public string DisplayText { get; }

        /// <summary>
        /// 목적:
        /// 원본 전체 텍스트
        /// - 장절 카드면 장절 전체
        /// - 본문 카드면 본문 전체
        /// </summary>
        public string FullText { get; }

        /// <summary>
        /// 목적:
        /// 카드 종류
        /// </summary>
        public VerseMatchCardType CardType { get; }

        /// <summary>
        /// 목적:
        /// 현재 선택 상태
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
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
        /// 현재 매칭 완료 상태
        /// </summary>
        public bool IsMatched
        {
            get => _isMatched;
            set
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
        /// 선택 가능한 카드인지 간단히 판단한다.
        /// </summary>
        public bool CanSelect => !IsMatched;

        public VerseMatchCardItem(
            string pairKey,
            string displayText,
            string fullText,
            VerseMatchCardType cardType)
        {
            PairKey = pairKey ?? throw new ArgumentNullException(nameof(pairKey));
            DisplayText = displayText ?? string.Empty;
            FullText = fullText ?? string.Empty;
            CardType = cardType;
        }

        /// <summary>
        /// 목적:
        /// 카드를 선택 상태로 만든다.
        /// </summary>
        public void Select()
        {
            IsSelected = true;
        }

        /// <summary>
        /// 목적:
        /// 카드 선택 상태를 해제한다.
        /// </summary>
        public void Unselect()
        {
            IsSelected = false;
        }

        /// <summary>
        /// 목적:
        /// 이 카드가 정답 매칭 완료 상태가 되도록 표시한다.
        /// </summary>
        public void MarkMatched()
        {
            IsMatched = true;
            IsSelected = false;
            OnPropertyChanged(nameof(CanSelect));
        }

        /// <summary>
        /// 목적:
        /// 카드 상태를 초기화한다.
        /// </summary>
        public void Reset()
        {
            IsSelected = false;
            IsMatched = false;
            OnPropertyChanged(nameof(CanSelect));
        }
    }

    /// <summary>
    /// 목적:
    /// 카드 종류를 구분한다.
    /// </summary>
    public enum VerseMatchCardType
    {
        Reference = 0,
        VerseText = 1
    }
}