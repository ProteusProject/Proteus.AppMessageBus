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

using System.Threading.Tasks;
using Proteus.AppMessageBus.Portable;
using Proteus.AppMessageBus.Portable.Abstractions;
using TestingHarness.Portable.Abstractions;
using TestingHarness.Portable.Messages;
using TestingHarness.Portable.ViewModels;

namespace TestingHarness.Portable.Subscribers
{
    public class CounterIncrementedViewModelEventHandler
        : IHandleDurable<CounterIncrementedWithAckEvent>, IHandleDurable<CounterIncrementedWithoutAckEvent>
    {

        private readonly DurableMessageBus _bus;
        private readonly IManageViewModels _modelManager;

        public CounterIncrementedViewModelEventHandler(DurableMessageBus bus, IManageViewModels modelManager)
        {
            _bus = bus;
            _modelManager = modelManager;
        }

        public async void Handle(CounterIncrementedWithAckEvent message)
        {
            var viewModel = _modelManager.Get<CounterDisplayPageViewModel>();

            if (null == viewModel)
            {
                viewModel = new CounterDisplayPageViewModel();
            }

            viewModel.AcknowledgedCounter++;

            _modelManager.Put(viewModel);

            await _bus.Acknowledge(message);
        }

        public void Handle(CounterIncrementedWithoutAckEvent message)
        {
            var viewModel = _modelManager.Get<CounterDisplayPageViewModel>();

            if (null == viewModel)
            {
                viewModel = new CounterDisplayPageViewModel();
            }

            viewModel.UnacknowledgedCounter++;

            _modelManager.Put(viewModel);

            //intentionally do NOT acknowledge the message...that's the whole pt of this handler :)
            //_bus.Acknowledge(message);
        }
    }


}