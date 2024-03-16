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
        public string Origin { get; set; }
        public string Destination { get; set; }
        public List<Row> rows { get; set; }
    }
    public class Distance
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class Duration
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class Element
    {
        public string status { get; set; }
        public Duration duration { get; set; }
        public Distance distance { get; set; }
    }

    

    public class Row
    {
        public List<Element> elements { get; set; }
    }

}
