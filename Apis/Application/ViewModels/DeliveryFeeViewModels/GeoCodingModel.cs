using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.DeliveryFeeViewModels
{
    public class GeoCodingModel
    {
        public List<Result> results { get; set; }
        public string status { get; set; }
    }
    public class AddressComponent
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
    }

    public class Compound
    {
        public string district { get; set; }
        public string commune { get; set; }
        public string province { get; set; }
    }

    public class Geometry
    {
        public Location location { get; set; }
        public object boundary { get; set; }
    }

    public class Location
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class PlusCode
    {
        public string compound_code { get; set; }
        public string global_code { get; set; }
    }

    public class Result
    {
        public List<AddressComponent> address_components { get; set; }
        public string formatted_address { get; set; }
        public Geometry geometry { get; set; }
        public string place_id { get; set; }
        public string reference { get; set; }
        public PlusCode plus_code { get; set; }
        public Compound compound { get; set; }
        public List<object> types { get; set; }
        public string name { get; set; }
        public string address { get; set; }
    }
}
