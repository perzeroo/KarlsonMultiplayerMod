namespace KarlsonMultiplayer
{
    public abstract class Command
    {
        public Command(string name)
        {
            this.name = name;
        }
        
        public string name;

        public abstract void OnCommand(string[] args);
    }
}