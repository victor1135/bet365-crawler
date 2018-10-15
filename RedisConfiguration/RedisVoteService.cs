namespace RedisConfig
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;
    using StackExchange.Redis;

    public class RedisVoteService<T> : BaseService<T>, IRedisService<T>
    {
        internal readonly IDatabase Db;
        protected readonly IRedisConnectionFactory ConnectionFactory;

        public RedisVoteService(IRedisConnectionFactory connectionFactory)
        {
            this.ConnectionFactory = connectionFactory;
            this.Db = this.ConnectionFactory.Connection().GetDatabase();
        }

        public void Delete(string key)
        {
            //if (string.IsNullOrWhiteSpace(key) || key.Contains(":")) throw new ArgumentException("invalid key");

            //key = this.GenerateKey(key);
            this.Db.KeyDelete(key);
        }

        public T Get(string key)
        {
            // key = this.GenerateKey(key);
            var hash = this.Db.HashGetAll(key);
            return this.MapFromHash(hash);
        }

        //public List<T> GetList(string key)
        //{
        //    // key = this.GenerateKey(key);
        //    var hash = this.Db.HashGetAll(key);
        //    return this.MapFromHash(hash);
        //}

        public List<T> GetList(string key)
        {
            var vList = this.Db.ListRange(key);
            if(vList.Count() == 0)
            {
                return null;
            }
            List<T> result = new List<T>();
            foreach (var item in vList)
            {
                var model = JsonConvert.DeserializeObject<T>(item); //反序列化
                result.Add(model);
            }
            return result;
        }

        public void SaveList(string key, List<T> obj)
        {
            var batch = this.Db.CreateBatch();
            foreach(var item in obj)
            {
                this.Db.ListRightPushAsync(key, JsonConvert.SerializeObject(item));
            }
        }

        public void Save(string key, T obj)
        {
            if (obj != null)
            {
                var hash = this.GenerateHash(obj);
                key = this.GenerateKey(key);

                if (this.Db.HashLength(key) == 0)
                {
                    this.Db.HashSet(key, hash);
                }
                else
                {
                    var props = this.Properties;
                    foreach (var item in props)
                    {
                        if (this.Db.HashExists(key, item.Name))
                        {   
                            this.Db.HashIncrement(key, item.Name, Convert.ToInt32(item.GetValue(obj)));
                        }
                    }
                }

            }
        }

        public void Flush(List<string> keys)
        {
            foreach (var key in keys)
            {
                this.Db.KeyDelete(key);
            }
        }
   }
}