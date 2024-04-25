using Application.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;
using Org.BouncyCastle.Utilities;
using Application.ViewModels.DeliveryFeeViewModels;
using System.Net.Http;
using AutoMapper.Execution;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Security.Cryptography;

namespace Application.Services
{
    public class DeliveryFeeService : IDeliveryFeeService
    {
        private readonly IUnitOfWork _unit;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public DeliveryFeeService(IUnitOfWork unit, HttpClient httpClient, IConfiguration configuration)
        {
            _unit = unit;
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task CreateAsync(IFormFile file)
        {
            var list = await _unit.DeliveryFeeRepository.GetAllAsync();
            if (list != null && list.Count > 0)
            {
                throw new Exception("Hiện bảng giá giao hàng đã tồn tại. Vì vậy không thể thêm mới!");
            }
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        //Kiểm tra trang tính có null ko
                        var worksheet = package.Workbook.Worksheets[0];
                        //Điếm số hàng có giá trị
                        int rowCount = worksheet.Dimension.Rows;
                        var listPrice = new List<DeliveryFee>();
                        for (int row = 2; row < rowCount; row++)
                        {
                            double maxDistance = (double)(worksheet.Cells[row, 1].Value);
                            for (int i = 1; i <= 3; i++)
                            {
                                var price = (double)worksheet.Cells[row, i + 1].Value;
                                var type = (DeliverySize)i;
                                DeliveryFee fee = new DeliveryFee() { DeliverySize = type, MaxDistance = maxDistance, Fee = price };
                                listPrice.Add(fee);
                            }
                        }
                        await _unit.DeliveryFeeRepository.AddRangeAsync(listPrice);
                        await _unit.SaveChangeAsync();
                    }
                }
            }
            catch (InvalidDataException ex)
            {
                throw new Exception("Dữ liệu trong file không đúng định dạng!");
            }
            catch (IOException ex)
            {
                throw new Exception("Đã xảy ra lỗi khi truy cập vào file!");
            }
        }

        public async Task UpdateAsync(IFormFile file)
        {
            _unit.BeginTransaction();
            try
            {
                var list = await _unit.DeliveryFeeRepository.GetAllAsync();
                _unit.DeliveryFeeRepository.HardDeleteRange(list);
                await _unit.SaveChangeAsync();
                await CreateAsync(file);
                await _unit.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _unit.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }
        public string FormatMoney(double price)
        {
            string formattedAmount = string.Format("{0:N0}", price);
            return formattedAmount.Replace(",", ".");
        }
        public async Task<DeliveryFeeDisplayModel> GetAllAsync()
        {
            var list = new List<DeliveryFee>();
            List<string> listMaxDistance = new List<string>();

            var table = new DeliveryFeeDisplayModel();
            table.Rows = new List<ItemDeliveryFee>();

            var tempDistance = await _unit.DeliveryFeeRepository.GetAllQueryable().Where(x => x.MaxDistance != null).OrderBy(x => x.MaxDistance).Select(x => x.MaxDistance).Distinct().ToListAsync();
            if (tempDistance == null || tempDistance.Count() == 0)
                throw new Exception("Hiện chưa có bảng giá vận chuyển Bonsai");
            int maxRow = 3;
            for (int i = 0; i < maxRow; i++)
            {
                var listItem = new ItemDeliveryFee();
                listItem.Items = new List<string>();
                if (i == 0)
                {
                    listItem.Items.Add("Khoảng cách (km)");
                    listItem.Items.Add("Kích thước bonsai nhỏ");
                    listItem.Items.Add("Kích thước bonsai vừa");
                    listItem.Items.Add("Kích thước bonsai lớn");
                    table.Rows.Add(listItem);
                }
                else if (i == maxRow - 1)
                {
                    var maxDistance = tempDistance.LastOrDefault();
                    var listPrice = await _unit.DeliveryFeeRepository.GetAllQueryable().Where(x=> x.MaxDistance == maxDistance).OrderBy(x => x.DeliverySize).Select(x => x.Fee).ToListAsync();
                    for (int j = 0; j < listPrice.Count + 1; j++)
                    {
                        if (j == 0)
                        {
                            listItem.Items.Add("từ km thứ " +( tempDistance.LastOrDefault()+1) + "  trở đi");
                        }
                        else
                        {
                            var price = FormatMoney(listPrice[j - 1]);
                            listItem.Items.Add(price + " VND");
                        }
                    }
                    table.Rows.Add(listItem);
                }
                else
                {
                    for (int a = 0; a < tempDistance.Count-1; a++)
                    {

                        var listPrice = await _unit.DeliveryFeeRepository.GetAllQueryable().Where(x => x.MaxDistance == tempDistance[a]).OrderBy(x => x.DeliverySize).Select(e => e.Fee).ToListAsync();
                        var items = new ItemDeliveryFee();
                        items.Items = new List<string>();
                        for (int j = 0; j < listPrice.Count + 1; j++)
                        {
                            if (j == 0)
                            {
                                if (a == 0)
                                    items.Items.Add("từ km đầu tiên");
                                else 
                                    items.Items.Add("từ km thứ " +( tempDistance[a] + 1));
                            }
                            else
                            {
                                var price = FormatMoney(listPrice[j - 1]);
                                items.Items.Add(price + " VND");
                            }
                        }
                        table.Rows.Add(items);
                    }
                }
            }
            return table;
        }

        public async Task<AddressModel> GetGeocoding(string address)
        {
            string codeKey = _configuration["GoongAPI"];
            HttpResponseMessage response = await _httpClient.GetAsync($"https://rsapi.goong.io/geocode?address={address}&api_key={codeKey}");
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<GeoCodingModel>(content);
                if (res.status.Equals("OK"))
                {
                    var addressModel = new AddressModel();
                    addressModel.Address = res.results[0].formatted_address;
                    addressModel.Geocoding = res.results[0].geometry.location.lat.ToString() + "," + res.results[0].geometry.location.lng.ToString();
                    return addressModel;
                }
                else
                    throw new Exception("Địa điểm giao hàng không hợp lệ.");
            }
            else
                throw new Exception("Địa điểm giao hàng không hợp lệ.");
        }

        public async Task<FeeViewModel> CalculateFee(string destination, IList<Guid> listBonsaiId)
        {
            string origin = _configuration["Origin:GeoLocation"];
            int distance = 0;
            var finalFee = new FeeViewModel();
            var addressModel = await GetGeocoding(destination);
            string codeKey = _configuration["GoongAPI"];
            int duration = 0;
            HttpResponseMessage response = await _httpClient.GetAsync($"https://rsapi.goong.io/DistanceMatrix?origins={origin}&destinations={addressModel.Geocoding}&vehicle=car&api_key={codeKey}");
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<DistanceResponse>(content);
                if (res != null && res.rows != null && res.rows.Count > 0)
                {
                    distance = res.rows.FirstOrDefault().elements.FirstOrDefault().distance.value / 1000;
                    duration = res.rows.FirstOrDefault().elements.FirstOrDefault().duration.value;
                }
                else
                {
                    throw new Exception("Địa điểm giao hàng không hợp lệ.");
                }
                finalFee.Origin_addresses = "372b QL20, Liên Nghĩa, Đức Trọng, Lâm Đồng, Vietnam";
                finalFee.Destination_addresses = addressModel.Address;
            }
            else
            {
                throw new Exception("Địa điểm giao hàng không hợp lệ.");
            }
            if (distance == 0) throw new Exception("Địa chỉ giao hàng không hợp lệ");
            try
            {
                finalFee.DurationHour = duration / 3600;
                finalFee.DurationMinute = (duration / 60) % 60;
                finalFee.ExpectedDeliveryDate = DateTime.Now.AddHours(finalFee.DurationHour).AddMinutes(finalFee.DurationMinute);
                double deliveryFee = 0;
                double bonsaiPrice = 0;
                foreach (var item in listBonsaiId.Distinct())
                {
                    var bonsai = await _unit.BonsaiRepository.GetAllQueryable().FirstOrDefaultAsync(x => x.Id ==item  && x.isDisable == false);
                    if (bonsai == null) throw new Exception("Không tìm thấy bonsai mà bạn cần. ");
                    if(bonsai.isSold == null|| bonsai.isSold == true ) throw new Exception("Không tìm thấy Bonsai mà bạn cần. ");
                    bonsaiPrice += bonsai.Price;
                    var fee = await CalculateFeeOfBonsai(bonsai.DeliverySize.Value, distance);
                    deliveryFee +=fee;
                }
                finalFee.DeliveryFee = deliveryFee;
                finalFee.PriceAllBonsai = bonsaiPrice;
                finalFee.FinalPrice  = bonsaiPrice + deliveryFee;
                finalFee.Distance = distance;
                return finalFee;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<FeeViewModel> CalculateFeeOrder(string destination, IList<Guid> listBonsaiId)
        {
            string origin = _configuration["Origin:GeoLocation"];
            int distance = 0;
            var finalFee = new FeeViewModel();
            var addressModel = await GetGeocoding(destination);
            string codeKey = _configuration["GoongAPI"];
            int duration = 0;
            HttpResponseMessage response = await _httpClient.GetAsync($"https://rsapi.goong.io/DistanceMatrix?origins={origin}&destinations={addressModel.Geocoding}&vehicle=car&api_key={codeKey}");
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<DistanceResponse>(content);
                if (res != null && res.rows != null && res.rows.Count > 0)
                {
                    distance = res.rows.FirstOrDefault().elements.FirstOrDefault().distance.value / 1000;
                    duration = res.rows.FirstOrDefault().elements.FirstOrDefault().duration.value;
                }
                else
                {
                    throw new Exception("Địa điểm giao hàng không hợp lệ.");
                }
                finalFee.Origin_addresses = "372b QL20, Liên Nghĩa, Đức Trọng, Lâm Đồng, Vietnam";
                finalFee.Destination_addresses = addressModel.Address;
            }
            else
            {
                throw new Exception("Địa điểm giao hàng không hợp lệ.");
            }
            if (distance == 0) throw new Exception("Địa chỉ giao hàng không hợp lệ");
            try
            {
                finalFee.DurationHour = duration / 3600;
                finalFee.DurationMinute = (duration / 60) % 60;
                finalFee.ExpectedDeliveryDate = DateTime.Now.AddHours(finalFee.DurationHour).AddMinutes(finalFee.DurationMinute);
                double deliveryFee = 0;
                double bonsaiPrice = 0;
                foreach (var item in listBonsaiId.Distinct())
                {
                    var bonsai = await _unit.BonsaiRepository.GetAllQueryable().FirstOrDefaultAsync(x => x.Id == item );
                    if (bonsai == null) throw new Exception("Không tìm thấy bonsai mà bạn cần. ");
                    bonsaiPrice += bonsai.Price;
                    var fee = await CalculateFeeOfBonsai(bonsai.DeliverySize.Value, distance);
                    deliveryFee += fee;
                }
                finalFee.DeliveryFee = deliveryFee;
                finalFee.PriceAllBonsai = bonsaiPrice;
                finalFee.FinalPrice = bonsaiPrice + deliveryFee;
                finalFee.Distance = distance;
                return finalFee;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<double> TestCalcutale(/*Guid bonsaiId,*/ int distance)
        {
            double price = 0;
            /*var bonsai = await _unit.BonsaiRepository.GetAllQueryable().FirstOrDefaultAsync(x => x.Id == bonsaiId);
            if (bonsai == null) throw new Exception("Không tìm thấy Bonsai mà bạn cần. ");*/
            DeliverySize size = DeliverySize.Small;
            var listFee = await _unit.DeliveryFeeRepository.GetAllQueryable().Where(x => x.DeliverySize == size).OrderBy(x=>x.MaxDistance).ToListAsync();
            var pointDistance = await _unit.DeliveryFeeRepository.GetAllQueryable().Where(x=>x.DeliverySize == size).Select(x=>x.MaxDistance).OrderBy(s=>s.Value).Distinct().ToListAsync();
            for (var i = 0;i< pointDistance.Count;i++)
            {
                if (i == 0 )
                {
                    if(distance <= pointDistance[i + 1])
                    {
                        price = listFee[i].Fee * distance; 
                        break;
                    }
                    else
                    {
                        price = price + (double)(listFee[i].Fee * (pointDistance[i + 1]));
                    }
                }
                else if (i == pointDistance.Count-1)
                {
                    price = price + (double)(listFee[i].Fee * (distance - pointDistance[i]));
                }
                else
                {
                    if(distance<= pointDistance[i +1])
                    {
                        price = price+ (distance - (double) pointDistance[i]) * listFee[i].Fee;
                        break;
                    }
                    else
                    {
                       price =  price + ((((double)pointDistance[i +1] - (double)pointDistance[i])) * listFee[i].Fee);
                      
                    }
                }
            }
            return price;
        }

        public async Task<double> CalculateFeeOfBonsai(DeliverySize size, int distance)
        {
            double price = 0;
            var listFee = await _unit.DeliveryFeeRepository.GetAllQueryable().Where(x => x.DeliverySize == size).OrderBy(x => x.MaxDistance).ToListAsync();
            var pointDistance = await _unit.DeliveryFeeRepository.GetAllQueryable().Where(x => x.DeliverySize == size).Select(x => x.MaxDistance).OrderBy(s => s.Value).Distinct().ToListAsync();
            for (var i = 0; i < pointDistance.Count; i++)
            {
                if (i == 0)
                {
                    if (distance <= pointDistance[i + 1])
                    {
                        price = listFee[i].Fee * distance;
                        break;
                    }
                    else
                    {
                        price = price + (double)(listFee[i].Fee * (pointDistance[i + 1]));
                    }
                }
                else if (i == pointDistance.Count - 1)
                {
                    price = price + (double)(listFee[i].Fee * (distance - pointDistance[i]));
                }
                else
                {
                    if (distance <= pointDistance[i + 1])
                    {
                        price = price + (distance - (double)pointDistance[i]) * listFee[i].Fee;
                        break;
                    }
                    else
                    {
                        price = price + ((((double)pointDistance[i + 1] - (double)pointDistance[i])) * listFee[i].Fee);

                    }
                }
            }
            return price;
        }
        public async Task<DistanceResponse> GetDistanse(string destination)
        {
            string origin = _configuration["Origin:GeoLocation"];
            var addressModel = await GetGeocoding(destination);
            string codeKey = _configuration["GoongAPI"];
            HttpResponseMessage response = await _httpClient.GetAsync($"https://rsapi.goong.io/DistanceMatrix?origins={origin}&destinations={addressModel.Geocoding}&vehicle=car&api_key={codeKey}");
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<DistanceResponse>(content);
                if (res != null && res.rows != null && res.rows.Count > 0)
                {
                    res.Origin = _configuration["Origin:Address"];
                    res.Destination = destination;
                    return res;
                }
                else
                {
                    throw new Exception("Địa điểm giao hàng không hợp lệ.");
                }
            }
            else
            {
                throw new Exception("Địa điểm giao hàng không hợp lệ.");
            }
        }
    }
}
