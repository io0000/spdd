using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using watabou.glwrap;

namespace watabou.utils
{
    public class PixmapPacker : IDisposable
    {
        bool packToTexture;
        bool disposed;
        int pageWidth, pageHeight;
        int padding;
        bool duplicateBorder;
        List<Page> pages = new List<Page>();
        GuillotineStrategy packStrategy;

        public PixmapPacker(int pageWidth, int pageHeight)
        {
            this.pageWidth = pageWidth;
            this.pageHeight = pageHeight;
            this.padding = 1;
            this.duplicateBorder = false;
            this.packStrategy = new GuillotineStrategy();
        }

        public RectF Pack(Pixmap image)
        {
            if (disposed)
                return null;

            //var rect = new PixmapPackerRectangle(0, 0, image.GetWidth(), image.GetHeight());
            var rect = new RectF(0, 0, image.GetWidth(), image.GetHeight());

            Page page = packStrategy.Pack(this, rect);

            int rectX = (int)rect.left;
            int rectY = (int)rect.top;
            int rectWidth = (int)rect.Width();
            int rectHeight = (int)rect.Height();

            if (packToTexture && !duplicateBorder && page.texture != null && !page.dirty)
            {
                // 텍스쳐 부분변경
                page.texture.SubBitmap(image, rectX, rectY, rectWidth, rectHeight);
                //page.texture.Bind();
                //Gdx.gl.glTexSubImage2D(page.texture.glTarget, 0, rectX, rectY, rectWidth, rectHeight, image.getGLFormat(),
                //    image.getGLType(), image.getPixels());
            }
            else
            {
                page.dirty = true;
            }

            // page의 image에 image를 draw
            page.image.DrawPixmap(image, rectX, rectY);

            return rect;
        }

        public void Dispose()
        {
            //for (Page page : pages)
            //{
            //    if (page.texture == null)
            //    {
            //        page.image.dispose();
            //    }
            //}
            disposed = true;
        }

        public void UpdateTextureRegions(List<TextureRegion> regions, TextureMinFilter minFilter, TextureMagFilter magFilter, bool useMipMaps)
        {
            UpdatePageTextures(minFilter, magFilter, useMipMaps);
            while (regions.Count < pages.Count)
                regions.Add(new TextureRegion(pages[regions.Count].texture));
        }

        public void UpdatePageTextures(TextureMinFilter minFilter, TextureMagFilter magFilter, bool useMipMaps)
        {
            foreach (Page page in pages)
                page.UpdateTexture(minFilter, magFilter, useMipMaps);
        }

        public List<Page> GetPages()
        {
            return pages;
        }

        public bool GetPackToTexture()
        {
            return packToTexture;
        }

        public void SetPackToTexture(bool packToTexture)
        {
            this.packToTexture = packToTexture;
        }

        public class Page
        {
            internal Pixmap image;
            internal Texture texture;
            internal bool dirty;

            public Page(PixmapPacker packer)
            {
                image = new Pixmap(packer.pageWidth, packer.pageHeight);
                image.SetBlending(Pixmap.Blending.None);
                //image.setColor(packer.getTransparentColor());
                //image.fill();
            }

            public Texture GetTexture()
            {
                return texture;
            }

            /** Creates the texture if it has not been created, else reuploads the entire page pixmap to the texture if the pixmap has
             * changed since this method was last called.
             * @return true if the texture was created or reuploaded. */
            public bool UpdateTexture(TextureMinFilter minFilter, TextureMagFilter magFilter, bool useMipMaps)
            {
                if (texture != null)
                {
                    if (!dirty)
                        return false;

                    //texture.load(texture.getTextureData());
                    texture.Bitmap(image);
                }
                else
                {
                    //texture = new Texture(new PixmapTextureData(image, image.getFormat(), useMipMaps, false, true)) {
                    //@Override
                    //
                    //public void dispose()
                    //{
                    //    super.dispose();
                    //    image.dispose();
                    //}
                    texture = new Texture();
                    texture.Bitmap(image);

                    texture.Filter((int)minFilter, (int)magFilter);
                }

                dirty = false;
                return true;
            }
        }

        public class GuillotineStrategy
        {
            public Page Pack(PixmapPacker packer, RectF rect)
            {
                GuillotinePage page;

                int size = packer.pages.Count;
                if (size == 0)
                {
                    // Add a page if empty.
                    page = new GuillotinePage(packer);
                    packer.pages.Add(page);
                }
                else
                {
                    // Always try to pack into the last page.
                    page = (GuillotinePage)packer.pages[size - 1];
                }

                int padding = packer.padding;
                rect.right += padding;
                rect.bottom += padding;
                Node node = Insert(page.root, rect);
                if (node == null)
                {
                    // Didn't fit, pack into a new page.
                    page = new GuillotinePage(packer);
                    packer.pages.Add(page);
                    node = Insert(page.root, rect);
                }
                node.full = true;

                // left, top, right, bottom
                rect.Set(node.rect.left,
                    node.rect.top,
                    node.rect.left + node.rect.Width() - padding,
                    node.rect.top + node.rect.Height() - padding);

                return page;
            }

            private Node Insert(Node node, RectF rect)
            {
                if (!node.full && node.leftChild != null && node.rightChild != null)
                {
                    Node newNode = Insert(node.leftChild, rect);
                    if (newNode == null)
                        newNode = Insert(node.rightChild, rect);
                    return newNode;
                }
                else
                {
                    if (node.full)
                        return null;

                    if (node.rect.Width() == rect.Width() && node.rect.Height() == rect.Height())
                        return node;
                    if (node.rect.Width() < rect.Width() || node.rect.Height() < rect.Height())
                        return null;

                    node.leftChild = new Node();
                    node.rightChild = new Node();

                    int deltaWidth = (int)node.rect.Width() - (int)rect.Width();
                    int deltaHeight = (int)node.rect.Height() - (int)rect.Height();
                    if (deltaWidth > deltaHeight)
                    {
                        node.leftChild.rect.left = node.rect.left;
                        node.leftChild.rect.top = node.rect.top;
                        node.leftChild.rect.right = node.leftChild.rect.left + rect.Width();
                        node.leftChild.rect.bottom = node.leftChild.rect.top + node.rect.Height();

                        node.rightChild.rect.left = node.rect.left + rect.Width();
                        node.rightChild.rect.top = node.rect.top;
                        node.rightChild.rect.right = node.rightChild.rect.left + (node.rect.Width() - rect.Width());
                        node.rightChild.rect.bottom = node.rightChild.rect.top + node.rect.Height();
                    }
                    else
                    {
                        node.leftChild.rect.left = node.rect.left;
                        node.leftChild.rect.top = node.rect.top;
                        node.leftChild.rect.right = node.leftChild.rect.left + node.rect.Width();
                        node.leftChild.rect.bottom = node.leftChild.rect.top + rect.Height();

                        node.rightChild.rect.left = node.rect.left;
                        node.rightChild.rect.top = node.rect.top + rect.Height();
                        node.rightChild.rect.right = node.rightChild.rect.left + node.rect.Width();
                        node.rightChild.rect.bottom = node.rightChild.rect.top + (node.rect.Height() - rect.Height());
                    }

                    return Insert(node.leftChild, rect);
                }
            }

            private class Node
            {
                public Node leftChild;
                public Node rightChild;
                public RectF rect = new RectF();
                public bool full = false;
            }

            private class GuillotinePage : Page
            {
                internal Node root;

                public GuillotinePage(PixmapPacker packer)
                    : base(packer)
                {
                    root = new Node();
                    var rc = root.rect;

                    rc.left = packer.padding;
                    rc.top = packer.padding;
                    rc.right = rc.left + packer.pageWidth - packer.padding * 2;
                    rc.bottom = rc.top + packer.pageHeight - packer.padding * 2;
                }
            }
        }

        //private class PixmapPackerRectangle : RectF
        //{
        //    int offsetX, offsetY;
        //    int originalWidth, originalHeight;
        //
        //    public PixmapPackerRectangle(int x, int y, int width, int height)
        //        : base(x, y, x + width, y + height)
        //    {
        //        this.offsetX = 0;
        //        this.offsetY = 0;
        //        this.originalWidth = width;
        //        this.originalHeight = height;
        //    }
        //
        //    public PixmapPackerRectangle(int x, int y, int width, int height,
        //        int offsetX, int offsetY, int originalWidth, int originalHeight)
        //        : base(x, y, x + width, y + height)
        //    {
        //        this.offsetX = offsetX;
        //        this.offsetY = offsetY;
        //        this.originalWidth = originalWidth;
        //        this.originalHeight = originalHeight;
        //    }
        //}
    }
}