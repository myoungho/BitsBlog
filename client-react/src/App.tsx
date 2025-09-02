import { PostsList } from "./components/PostsList";
import { usePosts } from "./hooks/usePosts";

export default function App() {
  const posts = usePosts();
  console.log(posts); // posts 값 확인

  return (
    <div>
      <h1>게시판 (React + TS)</h1>
      <PostsList posts={posts} />
    </div>
  );
}
