using System;
using System.Text;

namespace Ray.Utils.Helpers
{
    public static class Try
    {
        //private static Logger _logger;
        static Try()
        {
            //_logger = new Logger();
        }


        public static void It(Action action)
        {
            try
            {
                action();
            }
            //catch (DbEntityValidationException ex)
            //{
            //    HandleEntityValidationException(ex);
            //    //_logger.Write(ex);
            //}
            catch (System.Exception)
            {
                //_logger.Write(ex);
                throw;
            }
        }


        public static T It<T>(Func<T> action)
        {
            try
            {
                var result = action();

                return result;
            }
            //catch (DbEntityValidationException ex)
            //{
            //    HandleEntityValidationException(ex);

            //    throw ex;
            //}
            catch (System.Exception)
            {
                //_logger.Write(ex);
                throw;
            }
        }

        #region Private Methods
        //TODO: ver como validar este handleexception para aspcore
        //private static void HandleEntityValidationException(DbEntityValidationException ex)
        //{
        //    var sb = new StringBuilder();

        //    foreach (var failure in ex.EntityValidationErrors)
        //    {
        //        sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
        //        foreach (var error in failure.ValidationErrors)
        //        {
        //            sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
        //            sb.AppendLine();
        //        }
        //    }

        //    //_logger.Write(sb.ToString());
        //}


        #endregion

    }
}
