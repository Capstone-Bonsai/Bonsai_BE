﻿using Application.Repositories;
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
                        for (int row = 2; row < rowCount; row++) // Bắt đầu từ hàng thứ 2 để bỏ qua tiêu đề
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
                    listItem.Items.Add("Kích thước nhỏ");
                    listItem.Items.Add("Kích thước vừa");
                    listItem.Items.Add("Kích thước lớn");
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
                            listItem.Items.Add("từ " + tempDistance.LastOrDefault() + " km trở đi");
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
                                    items.Items.Add("từ " + tempDistance[a] + " km tiếp theo");
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

        public async Task<FeeViewModel> CalculateFee(string destination, double price)
        {
            string origin = _configuration["Origin:GeoLocation"];
            double finalPrice = 0;
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
            try
            {
                DeliverySize deliveryType;
                var feewithPrice = await _unit.DeliveryFeeRepository.GetAllQueryable().Where(x => !x.IsDeleted).ToListAsync();
                if (feewithPrice == null)
                    deliveryType = DeliverySize.Large;
                else
                    deliveryType = feewithPrice.FirstOrDefault().DeliverySize;

                var feeWithDistance = await _unit.DeliveryFeeRepository.GetAllQueryable().Where(x => x.MaxDistance >= distance && !x.IsDeleted && x.DeliverySize == deliveryType).OrderBy(x => x.MaxDistance).ToListAsync();
                DeliveryFee fee = new DeliveryFee();
                if (feeWithDistance == null || feeWithDistance.Count == 0)
                {
                    fee = await _unit.DeliveryFeeRepository.GetAllQueryable().Where(x => x.MaxDistance == null && !x.IsDeleted && x.DeliverySize == deliveryType).OrderBy(x => x.MaxDistance).FirstOrDefaultAsync();
                }
                else
                {
                    fee = feeWithDistance.FirstOrDefault();
                }


                finalFee.DurationHour = duration / 3600;
                finalFee.DurationMinute = (duration / 60) % 60;
                finalFee.ExpectedDeliveryDate = DateTime.Now.AddHours(finalFee.DurationHour).AddMinutes(finalFee.DurationMinute);


                finalPrice = fee.Fee * distance;
                finalFee.deliveryFee = fee;
                finalFee.DeliveryType = fee.DeliverySize.ToString();
                finalFee.Price = finalPrice;
                finalFee.Distance = distance;
                return finalFee;
            }
            catch (Exception)
            {
                throw new Exception("Chưa cập nhật bảng giá vận chuyển");
            }
        }


        public async Task<double> CalculateDelivery()
        {
            return 0;
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
