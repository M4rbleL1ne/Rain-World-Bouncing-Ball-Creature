using UnityEngine;
using System;
using RWCustom;

namespace BouncingBallCreature;

sealed class BouncingBallGraphics
{
    internal BouncingBallGraphics()
    {
        On.SnailGraphics.InitiateSprites += (orig, self, sLeaser, rCam) =>
        {
            orig(self, sLeaser, rCam);
            if (self.snail.Bob())
            {
                var sl = sLeaser.sprites;
                sLeaser.sprites = new FSprite[10];
                Array.Copy(sl, sLeaser.sprites, sl.Length);
                sLeaser.sprites[6].element = Futile.atlasManager.GetElementWithName("BoBShellA");
                sLeaser.sprites[6].color = rCam.currentPalette.blackColor;
                sLeaser.sprites[7].element = Futile.atlasManager.GetElementWithName("BoBShellB");
                sLeaser.sprites[7].alpha = .75f;
                sLeaser.sprites[8] = new("BoBShellC") { color = self.snail.shellColor[1] };
                sLeaser.sprites[9] = new("Circle20")
                {
                    scaleX = self.snail.bodyChunks[0].rad / 10f,
                    anchorY = 0f,
                    rotation = Custom.AimFromOneVectorToAnother(new(rCam.room.lightAngle.x, 0f - rCam.room.lightAngle.y), new(0f, 0f)),
                    color = new(.003921569f, 0f, 0f)
                };
                for (int j = 0; j < 2; j++) sLeaser.sprites[4 + j].scale = 2f;
                self.AddToContainer(sLeaser, rCam, null);
            }
        };
        On.SnailGraphics.DrawSprites += (orig, self, sLeaser, rCam, timeStacker, camPos) =>
        {
            if (self.snail is null || (self.snail.Bob() && self.snail.dead))
            {
                foreach (var spr in sLeaser.sprites)
                {
                    spr.alpha = 0f;
                    spr.scale = 0f;
                }
                sLeaser.CleanSpritesAndRemove();
                return;
            }
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.snail.Bob())
            {
                sLeaser.sprites[8].x = sLeaser.sprites[7].x;
                sLeaser.sprites[8].y = sLeaser.sprites[7].y;
                sLeaser.sprites[8].rotation = sLeaser.sprites[7].rotation;
                sLeaser.sprites[8].scale = sLeaser.sprites[7].scale;
                sLeaser.sprites[9].x = Mathf.Lerp(self.snail.bodyChunks[0].lastPos.x, self.snail.bodyChunks[0].pos.x, timeStacker) - camPos.x;
                sLeaser.sprites[9].y = Mathf.Lerp(self.snail.bodyChunks[0].lastPos.y, self.snail.bodyChunks[0].pos.y, timeStacker) - camPos.y;
                sLeaser.sprites[9].scaleY = rCam.room.lightAngle.magnitude * .25f * self.shadowExtensionFac;
                var a = Mathf.Lerp(self.snail.get_lastAlpha(), self.snail.get_alpha(), timeStacker);
                sLeaser.sprites[7].alpha = a;
                sLeaser.sprites[8].alpha = a;
            }
        };
        On.SnailGraphics.AddToContainer += (orig, self, sLeaser, rCam, newContainer) =>
        {
            orig(self, sLeaser, rCam, newContainer);
            if (self.snail.Bob() && sLeaser.sprites.Length >= 10)
            {
                sLeaser.sprites[8].RemoveFromContainer();
                rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[8]);
                rCam.ReturnFContainer("Shadows").AddChild(sLeaser.sprites[9]);
            }
        };
        On.SnailGraphics.ApplyPalette += (orig, self, sLeaser, rCam, palette) =>
        {
            orig(self, sLeaser, rCam, palette);
            if (self.snail.Bob())
            {
                sLeaser.sprites[6].color = palette.blackColor;
                self.snail.shellColor[0] = palette.texture.GetPixel(30, 5 - self.snail.get_effectColorRND(0) * 2);
                self.snail.shellColor[1] = palette.texture.GetPixel(30, 5 - self.snail.get_effectColorRND(1) * 2);
                sLeaser.sprites[7].color = self.snail.shellColor[1];
                sLeaser.sprites[8].color = self.snail.shellColor[1];
                for (int j = 0; j < 2; j++) sLeaser.sprites[4 + j].color = self.snail.shellColor[0];
            }
        };
    }
}