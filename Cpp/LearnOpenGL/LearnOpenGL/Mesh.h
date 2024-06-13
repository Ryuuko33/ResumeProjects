#pragma once
#include <string>
#include <vector>
#include <glm/vec3.hpp>

#include "Shader.h"

#define MAX_BONE_INFLUENCE 4

struct Vertex
{
    // position
    glm::vec3 Position;
    // normal
    glm::vec3 Normal;
    // texCoords
    glm::vec2 TexCoords;
    // tangent
    glm::vec3 Tangent;
    // bitangent
    glm::vec3 Bitangant;
        // bone indices which will influence this vertex
        int m_BoneIDs[MAX_BONE_INFLUENCE];
        // weight from each bone
        float m_Weights[MAX_BONE_INFLUENCE];
};

struct Texture
{
    unsigned int id;
    std::string type;
    std::string path;
};

class Mesh
{
public:
    // Mesh Data
    std::vector<Vertex> vertices;
    std::vector<unsigned int> indices;
    std::vector<Texture> textures;

    unsigned int VAO;
    // Mesh Func
    Mesh( std::vector<Vertex> vs, std::vector<unsigned int> is, std::vector<Texture> ts) :
    vertices(vs), indices(is), textures(ts)
    {
        setupMesh();   
    }
    void Draw(Shader shader);
private:
    // Render Data
    unsigned int VBO, EBO;
    // Func
    void setupMesh();
};
