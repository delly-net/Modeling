using Delly.Modeling.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Delly.Modeling
{
    /// <summary>
    /// 默认实体建模工厂实现
    /// </summary>
    public sealed class DefaultEntityModelFactory : IEntityModelFactory
    {
        // 基础类型 Type 字段，用于泛型方法匹配
        private static readonly Type _stringType = typeof(string);
        private static readonly Type _int32Type = typeof(int);
        private static readonly Type _int64Type = typeof(long);
        private static readonly Type _booleanType = typeof(bool);
        private static readonly Type _doubleType = typeof(double);
        private static readonly Type _decimalType = typeof(decimal);
        private static readonly Type _dateTimeType = typeof(DateTime);
        private static readonly Type _guidType = typeof(Guid);

        private readonly Dictionary<string, IEntityModel> _models;
        private readonly List<IEntityModelSet> _modelSets;
        private readonly HashSet<IEntityModelSet> _registeredSets;

        /// <summary>
        /// 初始化默认实体建模工厂
        /// </summary>
        public DefaultEntityModelFactory()
        {
            _models = new Dictionary<string, IEntityModel>();
            _modelSets = new List<IEntityModelSet>();
            _registeredSets = new HashSet<IEntityModelSet>();
        }


        // 辅助方法：从建模集合中查找
#if NETSTANDARD2_0
        private IEntityModel TryGetModelFromModelSets<T>()
#else
        private IEntityModel? TryGetModelFromModelSets<T>()
#endif
        {
            foreach (var set in _modelSets)
            {
                var model = set.TryGetModel<T>();
                if (model != null) { return model; }
            }
            return null;
        }

        /// <summary>
        /// 获取指定类全称的实体模型
        /// </summary>
        /// <param name="fullName">类全称，格式为 {Namespace}.{ClassName}</param>
        /// <returns>实体模型实例，如不存在则返回 null</returns>
#if NETSTANDARD2_0
        public IEntityModel GetModel(string fullName)
#else
        public IEntityModel? GetModel(string fullName)
#endif
        {
            // 优先判断基础类型，直接返回静态实例
            switch (fullName)
            {
                case "":
                case "System.String":
                    return StringEntityModel.Instance;
                case "System.Int32":
                    return Int32EntityModel.Instance;
                case "System.Int64":
                    return Int64EntityModel.Instance;
                case "System.Boolean":
                    return BooleanEntityModel.Instance;
                case "System.Double":
                    return DoubleEntityModel.Instance;
                case "System.Decimal":
                    return DecimalEntityModel.Instance;
                case "System.DateTime":
                    return DateTimeEntityModel.Instance;
                case "System.Guid":
                    return GuidEntityModel.Instance;
                default:
                    // 从字典查找自定义实体模型
                    return _models.TryGetValue(fullName, out var model) ? model : null;
            }
        }

        /// <summary>
        /// 获取指定表名的实体模型
        /// </summary>
        /// <param name="tableName">表名/查询对象名称</param>
        /// <returns>实体模型实例，如不存在则返回 null</returns>
#if NETSTANDARD2_0
        public IEntityModel GetModelByTableName(string tableName)
#else
        public IEntityModel? GetModelByTableName(string tableName)
#endif
        {
            return _models.Values.FirstOrDefault(m => m.Name == tableName);
        }

        /// <summary>
        /// 获取所有实体模型
        /// </summary>
        /// <returns>所有已注册的实体模型</returns>
        public IEnumerable<IEntityModel> GetAllModels()
        {
            return _models.Values;
        }

        /// <summary>
        /// 获取指定命名空间的所有实体模型
        /// </summary>
        /// <param name="ns">命名空间</param>
        /// <returns>指定命名空间的所有实体模型</returns>
        public IEnumerable<IEntityModel> GetModelsByNamespace(string ns)
        {
            return _models.Values.Where(m => m.Namespace == ns);
        }

        /// <summary>
        /// 检查是否存在指定类全称的实体模型
        /// </summary>
        /// <param name="fullName">类全称</param>
        /// <returns>是否存在</returns>
        public bool HasModel(string fullName)
        {
            return _models.ContainsKey(fullName);
        }

        /// <summary>
        /// 获取模型总数
        /// </summary>
        public int Count => _models.Count;

        /// <summary>
        /// 添加建模集合
        /// </summary>
        /// <param name="modelSet">建模集合</param>
        public void AddSet(IEntityModelSet modelSet)
        {
            if (_registeredSets.Add(modelSet))
            {
                _modelSets.Add(modelSet);
            }
            var models = modelSet.GetModels();
            foreach (var model in models)
            {
                var fullName = $"{model.Namespace}.{model.ClassName}";
                _models[fullName] = model;
            }
        }

        /// <summary>
        /// 添加单个实体模型
        /// </summary>
        /// <param name="model">实体模型</param>
        public void Add(IEntityModel model)
        {
            var fullName = $"{model.Namespace}.{model.ClassName}";
            _models[fullName] = model;
        }

        /// <summary>
        /// 获取指定类型的实体模型
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>指定类型的实体模型</returns>
        /// <exception cref="System.NotSupportedException">当类型未找到时抛出</exception>
        public IEntityModel GetModel<T>()
        {
            var typeOfT = typeof(T);

            if (typeOfT == _stringType) { return StringEntityModel.Instance; }
            if (typeOfT == _int32Type) { return Int32EntityModel.Instance; }
            if (typeOfT == _int64Type) { return Int64EntityModel.Instance; }
            if (typeOfT == _booleanType) { return BooleanEntityModel.Instance; }
            if (typeOfT == _doubleType) { return DoubleEntityModel.Instance; }
            if (typeOfT == _decimalType) { return DecimalEntityModel.Instance; }
            if (typeOfT == _dateTimeType) { return DateTimeEntityModel.Instance; }
            if (typeOfT == _guidType) { return GuidEntityModel.Instance; }

            // 判断是否为泛型类型
            if (typeOfT.IsGenericType)
            {
                // 获取开放泛型定义类型
                var genericDefinition = typeOfT.GetGenericTypeDefinition();

                // 根据开放泛型定义类型名称获取对应的泛型定义模型
                IEntityModel definitionModel;
                switch (genericDefinition.Name)
                {
                    case "List`1":
                        definitionModel = ListEntityModel.Instance;
                        break;
                    case "Dictionary`2":
                        definitionModel = DictionaryEntityModel.Instance;
                        break;
                    default:
                        throw new NotSupportedException($"不支持的泛型类型: {genericDefinition.Name}");
                }

                // 获取泛型参数类型对应的模型
                var genericArguments = typeOfT.GetGenericArguments();
                var argumentModels = new IEntityModel[genericArguments.Length];

                for (int i = 0; i < genericArguments.Length; i++)
                {
                    argumentModels[i] = GetModelByType(genericArguments[i]);
                }

                // 通过泛型定义模型创建已构造的泛型实体模型
                return definitionModel.MakeGenericModel(argumentModels);
            }

            var result = TryGetModelFromModelSets<T>();
            if (result != null) { return result; }

            throw new NotSupportedException($"类型 {typeof(T).FullName} 未在已注册的建模集合中找到");
        }

        /// <summary>
        /// 尝试获取指定类型的实体模型
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>指定类型的实体模型，未找到时返回 null</returns>
#if NETSTANDARD2_0
        public IEntityModel TryGetModel<T>()
#else
        public IEntityModel? TryGetModel<T>()
#endif
        {
            var typeOfT = typeof(T);

            if (typeOfT == _stringType) { return StringEntityModel.Instance; }
            if (typeOfT == _int32Type) { return Int32EntityModel.Instance; }
            if (typeOfT == _int64Type) { return Int64EntityModel.Instance; }
            if (typeOfT == _booleanType) { return BooleanEntityModel.Instance; }
            if (typeOfT == _doubleType) { return DoubleEntityModel.Instance; }
            if (typeOfT == _decimalType) { return DecimalEntityModel.Instance; }
            if (typeOfT == _dateTimeType) { return DateTimeEntityModel.Instance; }
            if (typeOfT == _guidType) { return GuidEntityModel.Instance; }

            return TryGetModelFromModelSets<T>();
        }

        /// <summary>
        /// 根据类型获取实体模型
        /// </summary>
        /// <param name="type">类型对象</param>
        /// <returns>实体模型对象</returns>
        private IEntityModel GetModelByType(Type type)
        {
            // 根据基础类型返回对应的实体模型
            switch (type.Name)
            {
                case nameof(String):
                    return StringEntityModel.Instance;
                case nameof(Int32):
                    return Int32EntityModel.Instance;
                case nameof(Int64):
                    return Int64EntityModel.Instance;
                case nameof(Boolean):
                    return BooleanEntityModel.Instance;
                case nameof(Double):
                    return DoubleEntityModel.Instance;
                case nameof(Decimal):
                    return DecimalEntityModel.Instance;
                case nameof(DateTime):
                    return DateTimeEntityModel.Instance;
                case nameof(Guid):
                    return GuidEntityModel.Instance;
                default:
                    throw new NotSupportedException($"不支持的类型: {type.Name}");
            }
        }
    }
}