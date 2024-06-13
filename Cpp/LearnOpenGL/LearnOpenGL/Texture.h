#pragma once
#include <glad/glad.h>

class Texture
{
public:
    Texture(const char* texturePath, int internalformat, int wrapping = GL_REPEAT, int filter = GL_LINEAR);
    unsigned int get()
    {
        return ID;
    }

private:
    unsigned int ID = -1;
};
