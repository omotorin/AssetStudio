using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public abstract class Component : EditorExtension
    {
        public PPtr<GameObject> m_GameObject = null!;

        protected Component(ObjectReader reader) : base(reader)
        {
            m_GameObject = new PPtr<GameObject>(reader);
        }
    }
}
