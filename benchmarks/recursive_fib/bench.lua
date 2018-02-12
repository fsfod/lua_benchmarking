local function fib(n)
  if n < 2 then return 1 end
  return fib(n-2) + fib(n-1)
end

function run_iter(n)
    local num = fib(n)
    assert(num > 0)
    return true
end
