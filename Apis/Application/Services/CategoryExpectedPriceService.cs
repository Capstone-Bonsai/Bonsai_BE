using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CategoryExpectedPriceService : ICategoryExpectedPriceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryExpectedPriceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public double GetPrice(float height)
        {
            var price = _unitOfWork.CategoryExpectedPriceRepository.GetExpectedPrice(height);
            return price;

        }

        public async Task CreateAsync(IFormFile file)
        {
            var list = await _unitOfWork.CategoryExpectedPriceRepository.GetAllAsync();
            if (list != null && list.Count > 0)
            {
                throw new Exception("Hiện bảng giá dịch vụ chăm sóc cây đã tồn tại. Vì vậy không thể thêm mới!");
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
                        var listPrice = new List<CategoryExpectedPrice>();
                        try
                        {
                            for (int row = 2; row <= rowCount; row++) // Bắt đầu từ hàng thứ 2 để bỏ qua tiêu đề
                            {
                                double price = (double)worksheet.Cells[row, 2].Value;
                                float maxHeight = 0;
                                try
                                {
                                    maxHeight = float.Parse( worksheet.Cells[row, 1].Value.ToString());
                                    CategoryExpectedPrice fee = new CategoryExpectedPrice() { MaxHeight = maxHeight,ExpectedPrice = price };
                                    listPrice.Add( fee );

                                }
                                catch (Exception ex)
                                {
                                    if (row != rowCount) throw new Exception("File không đúng yêu cầu.");
                                    CategoryExpectedPrice fee = new CategoryExpectedPrice() {  ExpectedPrice = price };
                                    listPrice.Add(fee);
                                }
                               
                            }
                        }
                        catch (Exception)
                        {
                            throw new Exception("File không đúng yêu cầu.");
                        }
                           
                        await _unitOfWork.CategoryExpectedPriceRepository.AddRangeAsync(listPrice);
                        await _unitOfWork.SaveChangeAsync();
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
            _unitOfWork.BeginTransaction();
            try
            {
                var list = await _unitOfWork.CategoryExpectedPriceRepository.GetAllAsync();
                _unitOfWork.CategoryExpectedPriceRepository.HardDeleteRange(list);
                await _unitOfWork.SaveChangeAsync();
                await CreateAsync(file);
               await  _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception("Đã xảy ra lỗi");
            }
        }
    }
}
