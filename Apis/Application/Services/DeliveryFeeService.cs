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

namespace Application.Services
{
    public class DeliveryFeeService : IDeliveryFeeService
    {
        private readonly IUnitOfWork _unit;
        private readonly HttpClient _httpClient;

        public DeliveryFeeService(IUnitOfWork unit, HttpClient httpClient)
        {
            _unit = unit;
            _httpClient = httpClient;
        }
        public async Task CreateAsync(IFormFile file)
        {
            var list = await _unit.DeliveryFeeRepository.GetAllAsync();
            if (list != null && list.Count > 0)
            {
                throw new Exception("Hiện bảng giá giao hàng đã tồn tại. Vì vậy không thể thêm mới!");
            }
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;//sử dụng để đặt ngữ cảnh giấy phép sử dụng của gói ExcelPackage thành phi thương mại
            try
            {
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
                        for (int row = 3; row <= rowCount; row++) // Bắt đầu từ hàng thứ 3 để bỏ qua tiêu đề
                        {
                            int maxDistance = 0;
                            try
                            {
                                maxDistance = int.Parse(worksheet.Cells[row, 1].Value.ToString());
                                for (int i = 1; i <= 3; i++)
                                {
                                    double maxPrice = 0;
                                    try
                                    {
                                        maxPrice = (double)worksheet.Cells[2, i + 1].Value;
                                        var price = (double)worksheet.Cells[row, i + 1].Value;
                                        var type = (DeliveryType)i;
                                        DeliveryFee fee = new DeliveryFee() { DeliveryType = type, MaxDistance = maxDistance, MaxPrice = maxPrice, Fee = price };
                                        listPrice.Add(fee);
                                    }
                                    catch (Exception)
                                    {
                                        var price = (double)worksheet.Cells[row, i + 1].Value;
                                        var type = (DeliveryType)i;
                                        DeliveryFee fee = new DeliveryFee() { DeliveryType = type, MaxDistance = maxDistance, Fee = price };
                                        listPrice.Add(fee);
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                for (int i = 1; i <= 3; i++)
                                {
                                    double maxPrice = 0;
                                    try
                                    {
                                        maxPrice = (double)worksheet.Cells[2, i + 1].Value;
                                        var price = (double)worksheet.Cells[row, i + 1].Value;
                                        var type = (DeliveryType)i;
                                        DeliveryFee fee = new DeliveryFee() { DeliveryType = type, MaxPrice = maxPrice, Fee = price };
                                        listPrice.Add(fee);
                                    }
                                    catch (Exception)
                                    {
                                        var price = (double)worksheet.Cells[row, i + 1].Value;
                                        var type = (DeliveryType)i;
                                        DeliveryFee fee = new DeliveryFee() { DeliveryType = type, Fee = price };
                                        listPrice.Add(fee);
                                    }
                                }
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
            var list = await _unit.DeliveryFeeRepository.GetAllAsync();
            _unit.DeliveryFeeRepository.HardDeleteRange(list);
            await _unit.SaveChangeAsync();
            await CreateAsync(file);

        }

        public async Task<DeliveryFeeViewModel> GetAllAsync()
        {
            var list = new List<DeliveryFee>();
            var deliveryModel = new DeliveryFeeViewModel();
            List<string> listMaxDistance = new List<string>();

            var temp = await _unit.DeliveryFeeRepository.GetAllQueryable().Select(x => x.MaxDistance).Distinct().ToListAsync();
            foreach (var item in temp)
            {
                if (item != null)
                {
                    listMaxDistance.Add(item.ToString());
                }
                else
                {
                    var last = temp.Last();
                    listMaxDistance.Add($"Lớn hơn {last.ToString()}");
                }
            }
            return deliveryModel;
        }

        public async Task<FeeViewModel> CalculateFee(string destination, double price)
        {
            string origin = "372b QL20, Liên Nghĩa, Đức Trọng, Lâm Đồng, Vietnam";
            double finalPrice = 0;
            int distance = 0;
            var finalFee = new FeeViewModel();
            HttpResponseMessage response = await _httpClient.GetAsync($"https://maps.googleapis.com/maps/api/distancematrix/json?origins={origin}&destinations={destination}&key=AIzaSyD-mpcsm8z3RzVow60KPO4rSNRvuozFmt0");

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<DistanceResponse>(content);
                if(res != null && res.rows != null && res.rows.Count > 0)
                {
                    distance = res.rows.FirstOrDefault().elements.FirstOrDefault().distance.value / 1000;

                }
                else
                {
                    throw new Exception("Địa điểm giao hàng không hợp lệ.");
                }

                finalFee.Origin_addresses = res.origin_addresses.FirstOrDefault();
                finalFee.Destination_addresses = res.destination_addresses.FirstOrDefault();
            }
            else
            {
                throw new Exception("Địa điểm giao hàng không hợp lệ.");
            }
            DeliveryType deliveryType;
            var feewithPrice = await _unit.DeliveryFeeRepository.GetAllQueryable().Where(x => x.MaxPrice >= price && !x.IsDeleted).OrderBy(x => x.MaxPrice).ToListAsync();
            if (feewithPrice == null || feewithPrice.Count == 0)
                deliveryType = DeliveryType.LargeTruck;
            else
                deliveryType = feewithPrice.FirstOrDefault().DeliveryType;

            var feeWithDistance = await _unit.DeliveryFeeRepository.GetAllQueryable().Where(x => x.MaxDistance >= distance && !x.IsDeleted && x.DeliveryType == deliveryType).OrderBy(x=>x.MaxDistance).ToListAsync();
            DeliveryFee fee = new DeliveryFee();
            if(feeWithDistance == null || feeWithDistance.Count==0)
            {
                 fee = await _unit.DeliveryFeeRepository.GetAllQueryable().Where(x => x.MaxDistance ==null && !x.IsDeleted && x.DeliveryType == deliveryType).OrderBy(x => x.MaxDistance).FirstOrDefaultAsync();
            }
            else
            {
                fee = feeWithDistance.FirstOrDefault();
            }
            finalPrice = fee.Fee * distance;
            finalFee.deliveryFee = fee;
            finalFee.DeliveryType = fee.DeliveryType.ToString();
            finalFee.Price = finalPrice;
            finalFee.Distance = distance;
            return finalFee;
        }
    }
}
