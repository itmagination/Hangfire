// This file is part of Hangfire.
// Copyright © 2013-2014 Sergey Odinokov.
// 
// Hangfire is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as 
// published by the Free Software Foundation, either version 3 
// of the License, or any later version.
// 
// Hangfire is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public 
// License along with Hangfire. If not, see <http://www.gnu.org/licenses/>.

using System;
using Hangfire.Common;

namespace Hangfire.UnitOfWork
{
    public interface IUnitOfWorkManager
    {
        /// <summary>
        /// Called before job is processed.
        /// </summary>
        /// <param name="job">Job context object.</param>
        /// <returns>Unit of work context.</returns>
        object Begin(Job job);

        /// <summary>
        /// Called after job was processed, if an error has occurred the exception will be passed.
        /// <paramref name="context">Unit of work context.</paramref>
        /// <paramref name="ex">Exception if thrown, null otherwise.</paramref>
        /// </summary>
        void End(object context, Exception ex = null);
    }
}
