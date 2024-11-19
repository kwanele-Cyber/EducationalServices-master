using EducationalServices.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf.qrcode;
using QRCoder;
using Org.BouncyCastle.Utilities;

namespace EducationalServices
{
    public interface IVerificationCodeRepository<T>:IDisposable where T : VerificationCode
    {
        VerificationCode Generate(string userId);
        Task<List<T>> GetAllAsync(string userId);
        Task<List<VerificationCode>> GetAllAsync(Expression<Func<T, bool>> query);
        Task<VerificationCode> GetByIdAsync(Guid id);
        Task<VerificationCode> GetSingleAsync(Expression<Func<T, bool>> query);

        Task RemoveAsync(Expression<Func<T, bool>> query);

        Task RemoveAsync(Guid id);

        Task<bool> VerifyAsync(string userId, string code);

    }

    public class VerificationCodeRepository : IVerificationCodeRepository<VerificationCode>
    {
        private readonly ApplicationDbContext _context;



        public VerificationCodeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public VerificationCode Generate(string userId)
        {
            var code = new VerificationCode();


            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(code.Code, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                code.Base64Img = "data:image/png;base64," + Convert.ToBase64String(qrCode.GetGraphic(20));
            }


            code.UserId = userId;
            _context.verificationCodes.Add(code);
            _context.SaveChanges();

            return code;
        }

        public async Task<List<VerificationCode>> GetAllAsync(string userId)
        {
            return await _context.verificationCodes.Where(t => t.UserId == userId).ToListAsync();
        }

        public async Task<List<VerificationCode>> GetAllAsync(Expression<Func<VerificationCode, bool>> query)
        {
            return await _context.verificationCodes.Where(query).ToListAsync();
        }

        public async Task RemoveAsync(Guid id)
        {
            var item = await GetByIdAsync(id);

            _context.verificationCodes.Remove(item);
            await _context.SaveChangesAsync();

        }

        public async Task RemoveAsync(Expression<Func<VerificationCode, bool>> query)
        {
            var item = await GetSingleAsync(query);

            _context.verificationCodes.Remove(item);
            await _context.SaveChangesAsync();

        }

        public async Task<VerificationCode> GetByIdAsync(Guid id)
        {
            var item = await _context.verificationCodes.FirstOrDefaultAsync(t => t.Id == id);

            return item;

        }

        public async Task<VerificationCode> GetSingleAsync(Expression<Func<VerificationCode, bool>> query)
        {
            var item = await _context.verificationCodes.FirstOrDefaultAsync(query);

            return item;

        }

        public async Task<bool> VerifyAsync(string userId, string code)
        {
            var item = await GetSingleAsync(t => t.UserId == userId && t.Code == code);

            if (item != null)
            {
                item.Status = VerificationCodeStatus.VERIFIED;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }


        public static VerificationCodeRepository Create()
        {
            
            return new VerificationCodeRepository(new ApplicationDbContext());
            
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
