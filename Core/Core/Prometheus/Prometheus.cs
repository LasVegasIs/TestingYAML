using Prometheus;
using System;

namespace Prometheus
{
    // TODO: replace with typed histogram (common mistake with current prometheus api is to use wrong number of labels, so such wrappers encode most common cases)
    public class SumCounter<TLabel1, TLabel2>
    {
        Counter sum;
        Counter count;

        internal SumCounter(string name, string help, string labelName1, string labelName2)
        {
            sum = Metrics.CreateCounter(name + "_sum", help, labelName1, labelName2);
            count = Metrics.CreateCounter(name + "_count", help, labelName1, labelName2);
        }

        class SumCount : IDisposable
        {
            private ITimer timer;

            public SumCount(SumCounter<TLabel1, TLabel2> parent, TLabel1 labelValue1, TLabel2 labelValue2)
            {
                parent.count.WithLabels(labelValue1.ToString(), labelValue2.ToString()).Inc();

                this.timer = parent.sum.WithLabels(labelValue1.ToString(), labelValue2.ToString()).NewTimer();
            }

            public void Dispose()
            {
                timer.Dispose();
            }
        }

        public IDisposable Measure(TLabel1 labelValue1, TLabel2 labelValue2)
        {
            return new SumCount(this, labelValue1, labelValue2);
        }
    }

    public class MetricsExtensions
    {
        public static SumCounter<TLabel1, TLabel2> CreateSumCounter<TLabel1, TLabel2>(string name, string help, string labelName1, string labelName2)
        {
            return new SumCounter<TLabel1, TLabel2>(name, help, labelName1, labelName2);
        }
    }
}
