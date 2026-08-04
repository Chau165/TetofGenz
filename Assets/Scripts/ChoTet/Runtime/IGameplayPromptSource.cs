namespace ChoTet.Runtime
{
    public interface IGameplayPromptSource
    {
        bool IsModalOpen { get; }
        void SetPrompt(string value);
    }
}
