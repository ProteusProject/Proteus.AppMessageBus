#region License

/*
 * Copyright © 2013-2016 the original author or authors.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Collections.Generic;
using TestingHarness.Portable.Abstractions;

namespace TestingHarness.Portable
{
    public class ViewModelManager : IManageViewModels
    {
        private readonly Dictionary<Type, object> _viewModels = new Dictionary<Type, object>();

        public void Put<TModel>(TModel model)
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

        public TModel Get<TModel>()
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