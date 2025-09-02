import { Post } from '../hooks/usePosts';

interface PostsListProps {
  posts: Post[];
}

export function PostsList({ posts }: PostsListProps) {
  return (
    <div>
      {posts.map(p => (
        <article key={p.id}>
          <h3>{p.title}</h3>
          <p>{p.content}</p>
        </article>
      ))}
    </div>
  );
}
