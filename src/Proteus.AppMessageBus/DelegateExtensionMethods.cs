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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Proteus.AppMessageBus.Portable
{
    public static class DelegateExtensionMethods
    {
        public static bool CanBeAwaited(this Delegate theDelegate)
        {
            var target = theDelegate.Target;
            var targetTypeInfo = target.GetType().GetTypeInfo();
            var matchingDeclaredMethods = targetTypeInfo.DeclaredMethods.Where(m => m.Name == theDelegate.GetMethodInfo().Name).ToArray();

            if (!matchingDeclaredMethods.Any())
            {
                return false;
            }

            var methodInfo = matchingDeclaredMethods.First();

            var returnTypeInfo = methodInfo.ReturnType.GetTypeInfo();

            if (returnTypeInfo.IsGenericType)
            {
                return returnTypeInfo.GetGenericTypeDefinition() == typeof(Task<>);
            }

            return returnTypeInfo.AsType() == typeof(Task);
        }
    }
}