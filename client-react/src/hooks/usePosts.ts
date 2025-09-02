import { useEffect, useState } from 'react';

export interface Post {
  id: number;
  title: string;
  content: string;
}

export function usePosts() {
  const [posts, setPosts] = useState<Post[]>([]);

  useEffect(() => {
    fetch('/api/posts')
      .then(res => res.json() as Promise<Post[]>)
      .then(setPosts);
  }, []);

  return posts;
}
