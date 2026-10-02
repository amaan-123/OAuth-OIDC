import keycloak from "./keycloak";

function App() {
  return (
    <div>
      <h1>OAuth/OIDC Lab</h1>

      <p>Logged in as: {keycloak.tokenParsed?.preferred_username}</p>

      <button onClick={() => keycloak.logout()}>Logout</button>
    </div>
  );
}

export default App;
