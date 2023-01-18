using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeeMind.Services.Data;

public enum EditMode
{
    None,

    New,

    Modified,

    Deleted
}

//public class EditableModel<T> where T : class
//{
//    public EditableModel(T model, EditMode editMode = EditMode.None)
//    {
//        Model = model;
//        EditMode = editMode;
//    }

//    public T Model { get; }
//    public EditMode EditMode { get; set; }
//}
