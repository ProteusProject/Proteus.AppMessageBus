using System;
using System.Collections.Generic;
using TestingHarness.Portable.Abstractions;

namespace TestingHarness.Portable
{
    public class ViewModelManager : IManageViewModels
    {
        private readonly Dictionary<Type, object> _viewModels = new Dictionary<Type, object>();

        public void StoreViewModel<TModel>(TModel model)
        {
            if (_viewModels.ContainsKey(typeof(TModel)))
            {
                _viewModels[typeof(TModel)] = model;
            }
            else
            {
                _viewModels.Add(typeof(TModel), model);
            }
        }

        public TModel RetrieveViewModel<TModel>()
        {
            object value;
            _viewModels.TryGetValue(typeof(TModel), out value);

            try
            {
                return (TModel)value;
            }
            catch (Exception)
            {
                return default(TModel);
            }
        }
    }
}