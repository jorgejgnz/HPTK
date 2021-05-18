using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Avatar
{
    public sealed class HumanTorsoView
    {
        HumanTorsoModel model;

        public BoneView hips { get { if (model.hips) return model.hips.view; else return null; } }
        public BoneView spine { get { if (model.spine) return model.spine.view; else return null; } }
        public BoneView chest { get { if (model.chest) return model.chest.view; else return null; } }
        public BoneView head { get { if (model.head) return model.head.view; else return null; } }
        public PointView eyes { get { if (model.eyes) return model.eyes.view; else return null; } }
        public PointView headTop { get { if (model.headTop) return model.headTop.view; else return null; } }

        public HumanTorsoView(HumanTorsoModel model)
        {
            this.model = model;
        }
    }

    public sealed class HumanArmView
    {
        HumanArmModel model;

        public BoneView shoulder { get { if (model.shoulder) return model.shoulder.view; else return null; } }
        public BoneView upper { get { if (model.upper) return model.upper.view; else return null; } }
        public BoneView forearm { get { if (model.forearm) return model.forearm.view; else return null; } }
        public HandView hand { get { if (model.hand) return model.hand.specificView; else return null; } }

        public HumanArmView(HumanArmModel model)
        {
            this.model = model;
        }
    }

    public sealed class HumanLegView
    {
        HumanLegModel model;

        public BoneView thigh { get { if (model.thigh) return model.thigh.view; else return null; } }
        public BoneView calf { get { if (model.calf) return model.calf.view; else return null; } }
        public BoneView foot { get { if (model.foot) return model.foot.view; else return null; } }
        public PointView toes { get { if (model.toes) return model.toes.view; else return null; } }

        public HumanLegView(HumanLegModel model)
        {
            this.model = model;
        }
    }
}
