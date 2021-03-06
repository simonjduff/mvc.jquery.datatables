﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Web.Routing;
using System.Web.Script.Serialization;
using Mvc.JQuery.DataTables.Models;
using Mvc.JQuery.DataTables.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mvc.JQuery.DataTables
{
    public class DataTableConfigVm
    {
        public bool HideHeaders { get; set; }
        IDictionary<string, object> m_JsOptions = new Dictionary<string, object>();

        static DataTableConfigVm()
        {
            DefaultTableClass = "table table-bordered table-striped";
        }

        public static string DefaultTableClass { get; set; }
        public string TableClass { get; set; }

        public DataTableConfigVm(string id, string ajaxUrl, IEnumerable<ColDef> columns)
        {
            AjaxUrl = ajaxUrl;
            this.Id = id;
            this.Columns = columns;
            this.ShowSearch = true;
            this.ShowPageSizes = true;
            this.TableTools = true;
            ColumnFilterVm = new ColumnFilterSettingsVm(this);
            AjaxErrorHandler = 
                "function(jqXHR, textStatus, errorThrown)" + 
                "{ " + 
                    "window.alert('error loading data: ' + textStatus + ' - ' + errorThrown); " + 
                    "console.log(arguments);" + 
                "}";
        }

        public bool ShowSearch { get; set; }

        public string Id { get; private set; }

        public string AjaxUrl { get; private set; }

        public IEnumerable<ColDef> Columns { get; private set; }

        public IDictionary<string, object> JsOptions { get { return m_JsOptions; } }

       

        public string ColumnDefsString
        {
            get
            {
                return ConvertColumnDefsToJson(Columns.ToArray());
            }
        }
        public bool ColumnFilter { get; set; }

        public ColumnFilterSettingsVm ColumnFilterVm { get; set; }

        public bool TableTools { get; set; }

        public bool AutoWidth { get; set; }

        public JToken SearchCols
        {
            get
            {
                var initialSearches = Columns
                    .Select(c => c.Searchable & c.SearchCols != null ? c.SearchCols : null as object).ToArray();
                return new JArray(initialSearches);
            }
        }



        private string _dom;

        public string Dom
        {
            get
            {
                if (!string.IsNullOrEmpty(_dom))
                    return _dom;

                string str = "";
                if (this.ColVis)
                    str += "C";
                if (this.TableTools)
                    str += "T<\"clear\">";
                if (this.ShowPageSizes)
                    str += "l";
                if (this.ShowSearch)
                    str += "f";
                return str + "tipr";
            }

            set { _dom = value; }
        }

        public bool ColVis { get; set; }

        public string ColumnSortingString
        {
            get
            {
                return ConvertColumnSortingToJson(Columns);
            }
        }

        public bool ShowPageSizes { get; set; }

        public bool StateSave { get; set; }

        public string Language { get; set; }

        public string DrawCallback { get; set; }
        public LengthMenuVm LengthMenu { get; set; }
        public int? PageLength { get; set; }
        public string GlobalJsVariableName { get; set; }

        private bool _columnFilter;

        public bool FixedLayout { get; set; }
        public string AjaxErrorHandler { get; set; }

        public class _FilterOn<TTarget>
        {
            private readonly TTarget _target;
            private readonly ColDef _colDef;

            public _FilterOn(TTarget target, ColDef colDef)
            {
                _target = target;
                _colDef = colDef;

            }

            public TTarget Select(params string[] options)
            {
                _colDef.Filter.type = "select";
                _colDef.Filter.values = options.Cast<object>().ToArray();
                if (_colDef.Type.IsEnum)
                {
                    _colDef.Filter.values = _colDef.Type.EnumValLabPairs();
                }
                return _target;
            }
            public TTarget NumberRange()
            {
                _colDef.Filter.type = "number-range";
                return _target;
            }

            public TTarget DateRange()
            {
                _colDef.Filter.type = "date-range";
                return _target;
            }

            public TTarget Number()
            {
                _colDef.Filter.type = "number";
                return _target;
            }

            public TTarget CheckBoxes(params string[] options)
            {
                _colDef.Filter.type = "checkbox";
                _colDef.Filter.values = options.Cast<object>().ToArray();
                if (_colDef.Type.IsEnum)
                {
                    _colDef.Filter.values = _colDef.Type.EnumValLabPairs();
                }
                return _target;
            }

            public TTarget Text()
            {
                _colDef.Filter.type = "text";
                return _target;
            }

            public TTarget None()
            {
                _colDef.Filter = null;
                return _target;
            }
        }
        public _FilterOn<DataTableConfigVm> FilterOn<T>()
        {
            return FilterOn<T>(null); 
        }
        public _FilterOn<DataTableConfigVm> FilterOn<T>(object jsOptions)
        {
            IDictionary<string, object> optionsDict = DataTableConfigVm.ConvertObjectToDictionary(jsOptions);
            return FilterOn<T>(optionsDict); 
        }
        ////public _FilterOn<DataTableConfigVm> FilterOn<T>(IDictionary<string, object> jsOptions)
        ////{
        ////    return new _FilterOn<DataTableConfigVm>(this, this.FilterTypeRules, (c, t) => t == typeof(T), jsOptions);
        ////}
        public _FilterOn<DataTableConfigVm> FilterOn(string columnName)
        {
            return FilterOn(columnName, null);
        }
        public _FilterOn<DataTableConfigVm> FilterOn(string columnName, object jsOptions)
        {
            IDictionary<string, object> optionsDict = ConvertObjectToDictionary(jsOptions);
            return FilterOn(columnName, optionsDict); 
        }
        public _FilterOn<DataTableConfigVm> FilterOn(string columnName, object jsOptions, object jsInitialSearchCols)
        {
            IDictionary<string, object> optionsDict = ConvertObjectToDictionary(jsOptions);
            IDictionary<string, object> initialSearchColsDict = ConvertObjectToDictionary(jsInitialSearchCols);
            return FilterOn(columnName, optionsDict, initialSearchColsDict);
        }
        public _FilterOn<DataTableConfigVm> FilterOn(string columnName, IDictionary<string, object> jsOptions)
        {
            return FilterOn(columnName, jsOptions, null);
        }
        public _FilterOn<DataTableConfigVm> FilterOn(string columnName, IDictionary<string, object> jsOptions, IDictionary<string, object> jsInitialSearchCols)
        {
            var colDef = this.Columns.Single(c => c.Name == columnName);
            if (jsOptions != null)
            {
                foreach (var jsOption in jsOptions)
                {
                    colDef.Filter[jsOption.Key] = jsOption.Value;
                }
            }
            if (jsInitialSearchCols != null)
            {
                colDef.SearchCols = new JObject();
                foreach (var jsInitialSearchCol in jsInitialSearchCols)
                {
                    colDef.SearchCols[jsInitialSearchCol.Key] = new JValue(jsInitialSearchCol.Value);
                }
            }
            return new _FilterOn<DataTableConfigVm>(this, colDef);
        }

        private static string ConvertDictionaryToJsonBody(IDictionary<string, object> dict)
        {
            // Converting to System.Collections.Generic.Dictionary<> to ensure Dictionary will be converted to Json in correct format
            var dictSystem = new Dictionary<string, object>(dict);
            var json = JsonConvert.SerializeObject((object)dictSystem, Formatting.None, new RawConverter());
            return json.Substring(1, json.Length - 2);
        }

        private static string ConvertColumnDefsToJson(ColDef[] columns)
        {
            Func<bool, bool> isFalse = x => x == false;
            Func<string, bool> isNonEmptyString = x => !string.IsNullOrEmpty(x);

            var defs = new List<dynamic>();

            defs.AddRange(ConvertColumnDefsToTargetedProperty(
                jsonPropertyName: "bSortable",
                propertySelector: column => column.Sortable,
                propertyPredicate: isFalse,
                columns: columns));
            defs.AddRange(ConvertColumnDefsToTargetedProperty(
                jsonPropertyName: "bVisible",
                propertySelector: column => column.Visible,
                propertyPredicate: isFalse,
                columns: columns));
            defs.AddRange(ConvertColumnDefsToTargetedProperty(
                jsonPropertyName: "bSearchable",
                propertySelector: column => column.Searchable,
                propertyPredicate: isFalse,
                columns: columns));
            defs.AddRange(ConvertColumnDefsToTargetedProperty(
                jsonPropertyName: "mRender",
                propertySelector: column => column.MRenderFunction,
                propertyConverter: x => new JRaw(x),
                propertyPredicate: isNonEmptyString,
                columns: columns));
            defs.AddRange(ConvertColumnDefsToTargetedProperty(
                jsonPropertyName: "className",
                propertySelector: column => column.CssClass,
                propertyPredicate: isNonEmptyString,
                columns: columns));
            defs.AddRange(ConvertColumnDefsToTargetedProperty(
                jsonPropertyName: "width",
                propertySelector: column => column.Width,
                propertyPredicate: isNonEmptyString,
                columns: columns));

            if (defs.Count > 0)
                return JsonConvert.SerializeObject(defs);

            return "[]";
        }

        private static string ConvertColumnSortingToJson(IEnumerable<ColDef> columns)
        {
            var sortList = columns.Select((c, idx) => c.SortDirection == SortDirection.None ? new dynamic[] { -1, "" } : (c.SortDirection == SortDirection.Ascending ? new dynamic[] { idx, "asc" } : new dynamic[] { idx, "desc" })).Where(x => x[0] > -1).ToArray();

            if (sortList.Length > 0) 
                return new JavaScriptSerializer().Serialize(sortList);

            return "[]";
        }

        private static IDictionary<string, object> ConvertObjectToDictionary(object obj)
        {
            // Doing this way because RouteValueDictionary converts to Json in wrong format
            return new Dictionary<string, object>(new RouteValueDictionary(obj));
        }

        private static IEnumerable<JObject> ConvertColumnDefsToTargetedProperty<TProperty>(
            string jsonPropertyName,
            Func<ColDef, TProperty> propertySelector,
            Func<TProperty, bool> propertyPredicate,
            IEnumerable<ColDef> columns)
        {
            return ConvertColumnDefsToTargetedProperty(
                jsonPropertyName,
                propertySelector,
                propertyPredicate,
                x => x,
                columns);
        }

        private static IEnumerable<JObject> ConvertColumnDefsToTargetedProperty<TProperty, TResult>(
            string jsonPropertyName,
            Func<ColDef, TProperty> propertySelector,
            Func<TProperty, bool> propertyPredicate,
            Func<TProperty, TResult> propertyConverter,
            IEnumerable<ColDef> columns)
        {
            return columns
                .Select((x, idx) => new { rawPropertyValue = propertySelector(x), idx })
                .Where(x => propertyPredicate(x.rawPropertyValue))
                .GroupBy(
                    x => x.rawPropertyValue,
                    (rawPropertyValue, groupedItems) => new
                    {
                        rawPropertyValue,
                        indices = groupedItems.Select(x => x.idx)
                    })
                .Select(x => new JObject(
                    new JProperty(jsonPropertyName, propertyConverter(x.rawPropertyValue)),
                    new JProperty("aTargets", new JArray(x.indices))
                    ));
        }
    }
}
