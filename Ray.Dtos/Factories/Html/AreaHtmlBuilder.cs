//using System.Reflection;

//namespace CMS.Dtos.Factories.Html
//{
//    public class AreaHtmlBuilder<T> where T : AreaDto
//    {
//        public T Model { get; set; }
//        public string Html { get; set; }

//        public AreaHtmlBuilder(T model, string html)
//        {
//            Html = html;
//            Model = model;
//        }

//        public string GenerateHtml()
//        {
//            if (Model.GetType() == typeof(AreaDto))
//            {
//                var dto = Model as AreaDto;
//                return new AreaHtml().GenerateHtml(dto, Html);
//            }

//            return string.Empty;
//        }

//    }

//    public class AreaHtml
//    {
//        public string GenerateHtml(AreaDto model, string html)
//        {
//            foreach (var pi in model.GetType().GetProperties())
//            {
//                html = html.ReplaceByPropertyName(pi.Name, pi.GetValue(model, null)?.ToString());
//            }
//            return html;
//        }
//    }
//}
