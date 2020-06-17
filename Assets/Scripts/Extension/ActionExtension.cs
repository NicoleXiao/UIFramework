using System;


namespace UIFramework
{
    public static class ActionExtension 
    {
        public static void TryInvoke<T>(this Action<T> action, T param)
        {
            if (action == null) return;
            try
            {
                action(param);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void TryInvoke(this Action action)
        {
            if (action == null) return;
            try
            {
                action();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
