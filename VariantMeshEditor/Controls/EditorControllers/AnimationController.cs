using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.Views.EditorViews;

namespace VariantMeshEditor.Controls.EditorControllers
{


    class AnimationController
    {
        AnimationEditorView _viewModel;

        public AnimationController(AnimationEditorView viewModel, AnimationElement animationElement, SkeletonElement skeletonElement)
        {
            _viewModel = viewModel;
        }
    }
}
