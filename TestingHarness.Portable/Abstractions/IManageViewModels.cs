namespace TestingHarness.Portable.Abstractions
{
    public interface IManageViewModels
    {
        void Put<TModel>(TModel model);
        TModel Get<TModel>();
    }
}