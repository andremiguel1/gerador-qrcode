using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace Aspc_QRCode.Controllers
{
    public class QRCodeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string qrTexto)
        {
            String cryptText = qrTexto.Crypt();
            cryptText = HttpUtility.UrlEncode(cryptText);
            String filePath = String.Format("wwwroot/qrr/-{0}-.qrr", cryptText);
            bool fileExists = false;
            
            String[] files = Directory.GetFiles("wwwroot/qrr");

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Contains(cryptText)) {
                    fileExists = true;
                    break;
                }
            }


            if (!fileExists)
            {
                QRCodeGenerator qrGerador = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGerador.CreateQrCode(qrTexto, QRCodeGenerator.ECCLevel.Q);

                qrCodeData.SaveRawData(filePath,
                       QRCodeData.Compression.Uncompressed);
            }

            QRCodeData qrCodeData1 = new QRCodeData(filePath,
                QRCodeData.Compression.Uncompressed);

            QRCode qrCode = new QRCode(qrCodeData1);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            //Graphics graphics = Graphics.FromImage(qrCodeImage);

            //var pen = new Pen(Color.Red, 20);
            //var rect = new Rectangle(200, 600, 600, 100);

            //graphics.SmoothingMode = SmoothingMode.AntiAlias;
            //graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            //graphics.DrawString("ALGUM TEXTO AQUI", new Font("Roboto", 50, FontStyle.Bold), Brushes.Red, rect);

            //graphics.DrawArc(pen, rect, 90, 90);

            ViewData["QrCodeText"] = qrTexto;

            return View(BitmapToBytes(qrCodeImage));
        }

        private static Byte[] BitmapToBytes(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public IActionResult ViewFile()
        {
            List<KeyValuePair<string, Byte[]>> fileData = new List<KeyValuePair<string, byte[]>>();

            KeyValuePair<string, Byte[]> data;
            string[] files = Directory.GetFiles("wwwroot/qrr");
            foreach (string file in files)
            {
                QRCodeData qrCodeData = new QRCodeData(file, QRCodeData.Compression.Uncompressed);

                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);

                Byte[] byteData = BitmapToBytes(qrCodeImage);

                String cryptText = Path.GetFileName(file).Split('-').GetValue(1).ToString();
                cryptText = HttpUtility.UrlDecode(cryptText);

                String qrCodeText = cryptText.Decrypt();

                data = new KeyValuePair<string, Byte[]>(qrCodeText, byteData);
                fileData.Add(data);
            }
            return View(fileData);
        }
    }
    public static class SimpleCryptUtil
    {
        private static byte[] key = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
        private static byte[] iv = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };

        public static string Crypt(this string text)
        {
            SymmetricAlgorithm algorithm = DES.Create();
            ICryptoTransform transform = algorithm.CreateEncryptor(key, iv);
            byte[] inputbuffer = Encoding.Unicode.GetBytes(text);
            byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return Convert.ToBase64String(outputBuffer);
        }

        public static string Decrypt(this string text)
        {
            SymmetricAlgorithm algorithm = DES.Create();
            ICryptoTransform transform = algorithm.CreateDecryptor(key, iv);
            byte[] inputbuffer = Convert.FromBase64String(text);
            byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return Encoding.Unicode.GetString(outputBuffer);
        }
    }
}



