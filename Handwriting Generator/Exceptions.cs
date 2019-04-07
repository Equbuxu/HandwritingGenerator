using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting_Generator
{
    public class FormException : Exception
    {
        public FormException() { }
        public FormException(string message) : base(message) { }
        public FormException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class MarkersNotFoundException : Exception
    {
        public MarkersNotFoundException() { }
        public MarkersNotFoundException(string message) : base(message) { }
        public MarkersNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
