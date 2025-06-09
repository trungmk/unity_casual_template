using UnityEngine;
using Core;
using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class TestNetworkService : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        _ = await GetPosts();
        _ = await GetPost();
        _ = SendPostRequest();
    }

    private async UniTask<List<Post>> GetPosts()
    {
        var posts = await ClientHttpService.GetAsync<List<Post>>("https://jsonplaceholder.typicode.com/posts");
        Debug.Log(posts);

        return posts;

        
    }

    private async UniTask<Post> GetPost()
    {
        var post = await ClientHttpService.GetAsync<Post>("https://jsonplaceholder.typicode.com/posts/1");
        Debug.Log(post);

        return post;
    }

    private async UniTask SendPostRequest()
    {
        // Test POST  
        var newPost = new Post { title = "Test Title", body = "Test Body", userId = 1 };
        var createdPost = await ClientHttpService.PostAsync<Post>("https://jsonplaceholder.typicode.com/posts", newPost);

        Debug.Log("test Post");
        Debug.Log("createdPost: " + createdPost);
    }
}

[Serializable]
public class Post
{
    public int id { get; set; }
    public string title { get; set; }
    public string body { get; set; }
    public int userId { get; set; }
}
