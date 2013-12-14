namespace TestingHarness.Portable.Abstractions
{
    public interface IManageViewModels
    {
        void StoreViewModel<TModel>(TModel model);
        TModel RetrieveViewModel<TModel>();
    }
}