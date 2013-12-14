namespace TestingHarness.Portable
{
    public class DisplayPageViewModel
    {
        public string Firstname { get; private set; }
        
        public string Lastname { get; private set; }

        public DisplayPageViewModel(string firstname, string lastname)
        {
            Firstname = firstname;
            Lastname = lastname;
        }
    }
}