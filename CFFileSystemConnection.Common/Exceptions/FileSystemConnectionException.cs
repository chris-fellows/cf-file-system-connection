using CFFileSystemConnection.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemConnection.Exceptions
{
    public class FileSystemConnectionException : Exception
    {        
        public ResponseErrorCodes? ResponseErrorCode { get; set; }

        public FileSystemConnectionException()
        {
        }

        public FileSystemConnectionException(string message) : base(message)
        {
        }

        public FileSystemConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }


        public FileSystemConnectionException(string message, params object[] args)
            : base(string.Format(CultureInfo.CurrentCulture, message, args))
        {
        }
    }
}
