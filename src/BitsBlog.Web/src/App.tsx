import { useEffect, useState } from 'react';
import { Button } from 'bits-ui';

interface Post {
  id: number;
  title: string;
  content: string;
}

export default function App() {
  const [posts, setPosts] = useState<Post[]>([]);

  useEffect(() => {
    fetch('/api/posts')
      .then(res => res.json())
      .then(setPosts);
  }, []);

  return (
    <div>
      <h1>게시판 (React + TypeScript)</h1>
      {posts.map(p => (
        <div key={p.id}>
          <h3>{p.title}</h3>
          <p>{p.content}</p>
        </div>
      ))}
      <Button>Bits UI 버튼</Button>
    </div>
  );
}
