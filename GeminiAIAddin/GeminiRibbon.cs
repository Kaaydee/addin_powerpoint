using Microsoft.Office.Tools.Ribbon;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;

namespace GeminiAIAddin
{
    public partial class GeminiRibbon
    {
        private async void buttonGenerate_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                string prompt = textboxPrompt.Text;
                if (string.IsNullOrWhiteSpace(prompt))
                {
                    MessageBox.Show("Vui lòng nhập prompt!");
                    return;
                }

                var gemini = new GeminiAI();

                // Gọi text generation
                string textResponse = await gemini.GenerateTextAsync(prompt);

                // Gọi image generation
                string imageResponse = await gemini.GenerateImageAsync(prompt);

                var slide = Globals.ThisAddIn.Application.ActiveWindow.View.Slide;

                // Thêm textbox chứa text (có thể cần parse JSON để lấy text thuần)
                slide.Shapes.AddTextbox(
                    Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal,
                    100, 50, 600, 200).TextFrame.TextRange.Text = ExtractTextFromResponse(textResponse);

                // Xử lý ảnh base64 từ response
                string base64Data = ExtractBase64ImageFromResponse(imageResponse);
                if (!string.IsNullOrEmpty(base64Data))
                {
                    string imagePath = await SaveBase64ImageAsync(base64Data);

                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        slide.Shapes.AddPicture(
                            imagePath,
                            Microsoft.Office.Core.MsoTriState.msoFalse,
                            Microsoft.Office.Core.MsoTriState.msoCTrue,
                            100, 300, 400, 300);
                    }
                    else
                    {
                        MessageBox.Show("Không lưu được ảnh từ base64.");
                    }
                }
                else
                {
                    MessageBox.Show("Không lấy được ảnh từ API.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private string ExtractTextFromResponse(string jsonResponse)
        {
            try
            {
                 var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                var candidate = root.GetProperty("candidates")[0];
                var content = candidate.GetProperty("content");
                var parts = content.GetProperty("parts");
                var text = parts[0].GetProperty("text").GetString();

                return text ?? "";
            }
            catch
            {
                return jsonResponse;
            }
        }

        private string ExtractBase64ImageFromResponse(string jsonResponse)
        {
            try
            {
                 var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                var candidate = root.GetProperty("candidates")[0];
                var content = candidate.GetProperty("content");
                var parts = content.GetProperty("parts");
                var base64String = parts[0].GetProperty("text").GetString();

                if (!string.IsNullOrWhiteSpace(base64String) && base64String.StartsWith("data:image"))
                {
                    int commaIndex = base64String.IndexOf(',');
                    return base64String.Substring(commaIndex + 1);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private async Task<string> SaveBase64ImageAsync(string base64Data)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64Data);
                string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "gemini_image.png");
                System.IO.File.WriteAllBytes(tempPath, imageBytes);

                return tempPath;
            }
            catch
            {
                return null;
            }
        }
    }
}
