import { useEffect, useState } from 'react';

export interface Post {
  id: number;
  title: string;
  content: string;
  created: string;
}

export function usePosts() {
  const [posts, setPosts] = useState<Post[]>([]);

  useEffect(() => {
    fetch('http://localhost:52015/api/posts')
      .then(res => res.json() as Promise<Post[]>)
      .then(setPosts);
  }, []);

  return posts;
}
