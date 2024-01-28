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

namespace Application.Services
{
    public class DeliveryFeeService : IDeliveryFeeService
    {
        private readonly IUnitOfWork _unit;

        public DeliveryFeeService(IUnitOfWork unit)
        {
            _unit = unit;
        }
        public async Task CreateAsync(IFormFile file)
        {
            var list = await _unit.DeliveryFeeRepository.GetAllAsync();
            if(list != null &&  list.Count > 0)
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

            var temp = await _unit.DeliveryFeeRepository.GetAllQueryable().Select(x=>x.MaxDistance).Distinct().ToListAsync();
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
    }
}
