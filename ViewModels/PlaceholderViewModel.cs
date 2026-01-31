namespace ScriptureTyping.ViewModels
{
    public  class PlaceholderViewModel
    {
        public string Title { get; }
        public string Message { get; }

        public PlaceholderViewModel(string title, string message)
        {
            Title = title;
            Message = message;
        }
    }
}
