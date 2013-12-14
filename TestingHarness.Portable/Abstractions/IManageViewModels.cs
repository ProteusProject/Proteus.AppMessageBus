namespace TestingHarness.Portable.Abstractions
{
    public interface IManageViewModels
    {
        void SetViewModelFor<TPage>(object model);
        object GetViewModelFor<TPage>();
    }
}