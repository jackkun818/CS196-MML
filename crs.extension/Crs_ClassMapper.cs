using CloneExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crs.extension
{
    public static class Crs_ClassMapper
    {
        /// <summary>
        /// 类型映射
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static TTarget ClassMapper<TTarget, TSource>(this TSource source)
        {
            try
            {
                var target = Activator.CreateInstance<TTarget>();
                var typeSource = source.GetType();
                var typeTarget = typeof(TTarget);

                foreach (var proSource in typeSource.GetProperties())
                {
                    foreach (var proTarget in typeTarget.GetProperties())
                    {
                        if (proTarget.Name == proSource.Name && proTarget.PropertyType == proSource.PropertyType && proTarget.Name != "Error" && proTarget.Name != "Item")
                        {
                            proTarget.SetValue(target, proSource.GetValue(source, null), null);
                        }
                    }
                }
                return target;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// 类型复制并映射
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static TTarget ClassCopyAndMapper<TTarget, TSource>(this TSource source)
        {
            try
            {
                var sourceClone = source.GetClone();
                var target = Activator.CreateInstance<TTarget>();
                var typeSource = sourceClone.GetType();
                var typeTarget = typeof(TTarget);

                foreach (var proSource in typeSource.GetProperties())
                {
                    foreach (var proTarget in typeTarget.GetProperties())
                    {
                        if (proTarget.Name == proSource.Name && proTarget.PropertyType == proSource.PropertyType && proTarget.Name != "Error" && proTarget.Name != "Item")
                        {
                            proTarget.SetValue(target, proSource.GetValue(sourceClone, null), null);
                        }
                    }
                }
                return target;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public static TSource ClassCopy<TSource>(this TSource source)
        {
            try
            {
                var sourceClone = source.GetClone();
                return sourceClone;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
