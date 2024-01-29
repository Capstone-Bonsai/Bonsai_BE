using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Application.ViewModels.DeliveryFeeViewModels
{
    public class DistanceResponse
    {
        public IList<string> destination_addresses {  get; set; }
        public IList<string> origin_addresses { get; set; }
        public IList<Row> rows { get; set; }
        public string status { get; set; }
    }
    public class Row
    {
        public IList<Element> elements { get; set; }
    }

    public class Distance
    {
        public string text { get; set; }
        public int value;
    }

    public class Duration
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class Element
    {
        public Distance distance { get; set; }
        public Duration duration { get; set; }
        public string status { get; set; }
    }
}
