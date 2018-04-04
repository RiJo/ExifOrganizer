//
// MediaQuerierException.cs: Custom exception class.
//
// Copyright (C) 2016 Rikard Johansson
//
// This program is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option) any
// later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with
// this program. If not, see http://www.gnu.org/licenses/.
//

using System;
using System.Runtime.Serialization;

namespace ExifOrganizer.Querier
{
    [Serializable]
    public class MediaQuerierException : Exception
    {
        public MediaQuerierException(string message, params object[] args)
            : base(String.Format(message, args))
        {
        }

        public MediaQuerierException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected MediaQuerierException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}