using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Controls
{
    public interface ICloseableControl<TResult>
    {
        event EventHandler<TResult> Closed;
    }
}
