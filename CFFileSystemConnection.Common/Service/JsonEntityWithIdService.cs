using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.Models;
using CFFileSystemConnection.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemConnection.Service
{
    public abstract class JsonEntityWithIdService<TEntity> : IEntityWithIdService<TEntity> where TEntity : class
    {
        protected readonly string _folder;
        protected readonly Func<TEntity, string> _getIdFunction;

        public JsonEntityWithIdService(string folder,
                            Func<TEntity, string> getIdFunction)
        {
            _folder = folder;
            Directory.CreateDirectory(folder);

            _getIdFunction = getIdFunction;
        }

        private string GetFileById(string id)
        {
            var typeName = typeof(TEntity).Name;
            return Path.Combine(_folder, $"{typeName}.{id}.json");
        }

        public TEntity? GetById(string id)
        {
            var file = GetFileById(id);

            return File.Exists(file) ?
                JsonUtilities.DeserializeFromString<TEntity>(File.ReadAllText(file, Encoding.UTF8), JsonUtilities.DefaultJsonSerializerOptions) :
                default(TEntity);
        }

        public List<TEntity> GetAll()
        {
            var entitities = new List<TEntity>();

            var typeName = typeof(TEntity).Name;
            foreach (var file in Directory.GetFiles(_folder, $"{typeName}.*.json"))
            {
                entitities.Add(JsonUtilities.DeserializeFromString<TEntity>(File.ReadAllText(file, Encoding.UTF8), JsonUtilities.DefaultJsonSerializerOptions));
            }

            return entitities;
        }
      
        public void Add(TEntity entity)
        {
            var file = GetFileById(_getIdFunction(entity));
            File.WriteAllText(file, JsonUtilities.SerializeToString(entity, JsonUtilities.DefaultJsonSerializerOptions));
        }

        public void Update(TEntity entity)
        {
            Add(entity);
        }

        public void Delete(string id)
        {
            var file = GetFileById(id);
            if (File.Exists(file)) File.Delete(file);
        }    
    }
}
